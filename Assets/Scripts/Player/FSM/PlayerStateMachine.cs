using System;
using System.Collections;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CharacterStatsSO _stats;

    // Components
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private CharacterStatsController _statsController;

    // State Machine
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    // Internal Data
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private float _time;
    private bool _isControlDisabled = false;

    // Detection & Timers
    private bool _grounded;
    private float _frameLeftGrounded = float.MinValue;
    private float _timeJumpWasPressed;
    private bool _canDash = true;
    private bool _isDashing;

    // Consumables (Flags para los estados)
    private bool _jumpToConsume;
    private bool _dashToConsume;
    private bool _bufferedJumpUsable;
    private bool _coyoteUsable;

    #region Properties & Interface

    // Events
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Dashed;

    // Getters & Setters para los Estados
    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public CharacterStatsSO Stats => _stats;
    public CharacterStatsController StatsController => _statsController;
    public Vector2 FrameVelocity { get => _frameVelocity; set => _frameVelocity = value; }
    public FrameInput FrameInput => _frameInput;

    public bool Grounded => _grounded;
    public bool IsDashing { get => _isDashing; set => _isDashing = value; }
    public bool CanDash { get => _canDash; set => _canDash = value; }
    public bool JumpToConsume { get => _jumpToConsume; set => _jumpToConsume = value; }
    public bool DashToConsume { get => _dashToConsume; set => _dashToConsume = value; }

    // Helper Properties
    public bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    public bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _statsController = GetComponent<CharacterStatsController>();
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        // Inicializar FSM
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded(); // Estado inicial
        _currentState.EnterState();
    }

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
            _frameVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckCollisions();

        // Ejecutar lógica de la FSM (incluye sub-estados)
        _currentState.UpdateStates();

        ApplyMovement();
    }

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

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;

            _jumpToConsume = false;
            _timeJumpWasPressed = 0;

            _statsController.RegenStamina();
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

    #region External Triggers (Called by States)

    public void UseJumpBuffer() => _bufferedJumpUsable = false;
    public void UseCoyote() => _coyoteUsable = false;
    public void InvokeJumpEvent() => Jumped?.Invoke();
    public void InvokeDashEvent() => Dashed?.Invoke();

    public void StartDashCooldown()
    {
        // Si ya hay un cooldown corriendo, no iniciamos otro
        StopCoroutine(nameof(DashCooldownRoutine));
        StartCoroutine(DashCooldownRoutine());
    }

    private IEnumerator DashCooldownRoutine()
    {
        _canDash = false;
        yield return new WaitForSeconds(_stats.DashCooldown);
        _canDash = true;
    }

    public void SetControl(bool state)
    {
        _isControlDisabled = !state;

        if (_isControlDisabled) // ENTRANDO A CINEMÁTICA
        {
            // 1. Matamos la velocidad interna y física
            _frameVelocity = Vector2.zero;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;

            // 2. IMPORTANTE: Forzamos el cambio de estado a Idle 
            // para que el DashState se cierre y ejecute su ExitState()
            _currentState = _states.Grounded();
            _currentState.EnterState();

            // 3. Lo hacemos cinemático para que nada lo mueva
            if (_rb != null) _rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else // AL SALIR DE LA CINEMÁTICA
        {
            // 1. Limpieza de velocidades residuales
            _frameVelocity = Vector2.zero;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }

            // 2. RESETEAR EL DASH (Aquí está la solución)
            IsDashing = false;    // Nos aseguramos de que no crea que está dasheando
            CanDash = true;       // Forzamos que el dash esté disponible de nuevo
            DashToConsume = false; // Limpiamos cualquier pulsación vieja

            // 3. Reiniciar FSM
            _currentState = _states.Grounded();
            _currentState.EnterState();
        }
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (_col == null) return;
        Gizmos.color = _grounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(_col.bounds.center + Vector3.down * _stats.GrounderDistance, _col.size);
    }
    #endregion
}