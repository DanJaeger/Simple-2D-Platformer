using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador principal del jugador basado en una Máquina de Estados Jerárquica (HFSM).
/// Actúa como el centro de datos (Hub) y motor físico, delegando la lógica a los estados.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerStateMachine : MonoBehaviour
{
    #region Serialized Fields
    [Header("Configuration")]
    [Tooltip("ScriptableObject que contiene los ajustes físicos del personaje.")]
    [SerializeField] private CharacterStatsSO _stats;
    #endregion

    #region Private Components
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private CharacterStatsController _statsController;
    private PlayerStateFactory _states;
    private PlayerBaseState _currentState;
    #endregion

    #region Internal State Data
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private float _time;
    private bool _isControlDisabled;

    // Detection & Flags
    private bool _grounded;
    private bool _canDash = true;
    private bool _isDashing;
    private bool _jumpToConsume;
    private bool _dashToConsume;
    private bool _bufferedJumpUsable;
    private bool _coyoteUsable;

    // Timers
    private float _frameLeftGrounded = float.MinValue;
    private float _timeJumpWasPressed;
    #endregion

    #region Interface & Events
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Dashed;

    // Propiedades expuestas para los estados
    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public CharacterStatsSO Stats => _stats;
    public CharacterStatsController StatsController => _statsController;
    public Vector2 FrameVelocity { get => _frameVelocity; set => _frameVelocity = value; }
    public FrameInput FrameInputRef => _frameInput;
    public bool Grounded => _grounded;
    public bool IsDashing { get => _isDashing; set => _isDashing = value; }
    public bool CanDash { get => _canDash; set => _canDash = value; }
    public bool JumpToConsume { get => _jumpToConsume; set => _jumpToConsume = value; }
    public bool DashToConsume { get => _dashToConsume; set => _dashToConsume = value; }

    // Helpers de tiempo para Salto y Coyote
    public bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    public bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;
    #endregion

    #region Initialization
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _statsController = GetComponent<CharacterStatsController>();

        // Optimización: Cachear configuración de colisiones de Unity
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();
    }
    #endregion

    #region Loop Principal
    private void Update()
    {
        if (_isControlDisabled) return;

        _time += Time.deltaTime;
        GatherInput();
    }

    private void FixedUpdate()
    {
        if (_isControlDisabled)
        {
            StopMovementImmediate();
            return;
        }

        CheckCollisions();
        _currentState.UpdateStates(); // Ejecuta lógica de la jerarquía de estados
        ApplyMovement();
    }
    #endregion

    #region Input Logic
    /// <summary>
    /// Recolecta el input del jugador y aplica normalización o Snapping según los stats.
    /// </summary>
    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || InputManager.Instance.Jump,
            JumpHeld = Input.GetButton("Jump") || InputManager.Instance.Jump,
            DashDown = Input.GetKeyDown(KeyCode.LeftShift) || InputManager.Instance.Dash,
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (_stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        if (_frameInput.DashDown && _canDash && !_isDashing)
        {
            _dashToConsume = true;
        }
    }
    #endregion

    #region Physics & Collisions
    /// <summary>
    /// Realiza detecciones mediante CapsuleCast para determinar estado de suelo y techo.
    /// </summary>
    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        HandleGroundTransition(groundHit);

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void HandleGroundTransition(bool groundHit)
    {
        if (!_grounded && groundHit) // Aterrizando
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _jumpToConsume = false;
            _timeJumpWasPressed = 0;

            _statsController.RegenStamina();
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (_grounded && !groundHit) // Dejando el suelo
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }
    }

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

    private void StopMovementImmediate()
    {
        _frameVelocity = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
    }
    #endregion

    #region Public State API
    public void UseJumpBuffer() => _bufferedJumpUsable = false;
    public void UseCoyote() => _coyoteUsable = false;
    public void InvokeJumpEvent() => Jumped?.Invoke();
    public void InvokeDashEvent() => Dashed?.Invoke();

    public void StartDashCooldown()
    {
        StopCoroutine(nameof(DashCooldownRoutine));
        StartCoroutine(DashCooldownRoutine());
    }

    private IEnumerator DashCooldownRoutine()
    {
        _canDash = false;
        yield return new WaitForSeconds(_stats.DashCooldown);
        _canDash = true;
    }

    /// <summary>
    /// Activa o desactiva el control del jugador. Útil para cinemáticas o menús.
    /// </summary>
    public void SetControl(bool state)
    {
        _isControlDisabled = !state;

        if (_isControlDisabled)
        {
            StopMovementImmediate();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            ResetToIdle();
        }
        else
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            ResetSkills();
            ResetToIdle();
        }
    }

    private void ResetToIdle()
    {
        _currentState = _states.Grounded();
        _currentState.EnterState();
    }

    private void ResetSkills()
    {
        _isDashing = false;
        _canDash = true;
        _dashToConsume = false;
        _jumpToConsume = false;
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (_col == null) return;
        Gizmos.color = _grounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(_col.bounds.center + Vector3.down * _stats.GrounderDistance, _col.size);
    }
    #endregion
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
    public Vector2 Move;
}