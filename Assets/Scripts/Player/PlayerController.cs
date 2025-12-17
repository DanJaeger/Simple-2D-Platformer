using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterStatsSO _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private CharacterStatsController _statsController;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time;
    private bool _isControlDisabled = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _statsController = GetComponent<CharacterStatsController>();

        //set the vlaue if collisions with initial colliders if they are colliding. example: Starts inside the character's body
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || InputManager.Instance.Jump,
            JumpHeld = Input.GetButton("Jump") || InputManager.Instance.Jump,
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
    }

    private void FixedUpdate()
    {
        if (_isControlDisabled)
        {
            ApplyMovement(); // Mantiene la gravedad si es necesario, o déjalo vacío
            return;
        }

        CheckCollisions();

        HandleJump();
        //HandleDash();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        // Hit a Ceiling
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            _statsController.RegenStamina();
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion


    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocityY > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.JumpPower;
        _statsController.ConsumeStamina(10f);
        Jumped?.Invoke();
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif

    public void SetControl(bool state)
    {
        _isControlDisabled = !state;
        if (_isControlDisabled)
        {
            // Detener al jugador inmediatamente para que no "deslice" en la cinemática
            _frameVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDrawGizmos()
    {
        if (_col == null) _col = GetComponent<CapsuleCollider2D>();
        if (_stats == null || _col == null) return;

        // 1. Usamos el centro de los bounds (que ya incluye la posición del objeto + offset del collider)
        Vector2 startCenter = _col.bounds.center;
        Vector2 castSize = _col.size;
        float distance = _stats.GrounderDistance;
        Vector2 endCenter = startCenter + Vector2.down * distance;

        // 2. Dibujar Cápsula de Origen (donde está el collider actualmente)
        Gizmos.color = _grounded ? Color.green : Color.red;
        DrawCapsule(startCenter, castSize, _col.direction);

        // 3. Dibujar Cápsula de Destino (el área que busca el suelo)
        // Reducimos un poco el tamaño visual para que se note el barrido
        Gizmos.color = Color.cyan;
        DrawCapsule(endCenter, castSize, _col.direction);

        // 4. Dibujar línea de trayectoria
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startCenter, endCenter);
    }

    private void DrawCapsule(Vector2 center, Vector2 size, CapsuleDirection2D direction)
    {
        float radius = size.x / 2f;
        float cylinderHeight = Mathf.Max(0, size.y - size.x);

        if (direction == CapsuleDirection2D.Vertical)
        {
            Vector3 topSphere = (Vector3)center + Vector3.up * (cylinderHeight / 2f);
            Vector3 bottomSphere = (Vector3)center + Vector3.down * (cylinderHeight / 2f);

            Gizmos.DrawWireSphere(topSphere, radius);
            Gizmos.DrawWireSphere(bottomSphere, radius);
            Gizmos.DrawLine(topSphere + Vector3.left * radius, bottomSphere + Vector3.left * radius);
            Gizmos.DrawLine(topSphere + Vector3.right * radius, bottomSphere + Vector3.right * radius);
        }
        else // Horizontal
        {
            Vector3 rightSphere = (Vector3)center + Vector3.right * (cylinderHeight / 2f);
            Vector3 leftSphere = (Vector3)center + Vector3.left * (cylinderHeight / 2f);

            Gizmos.DrawWireSphere(rightSphere, radius);
            Gizmos.DrawWireSphere(leftSphere, radius);
            Gizmos.DrawLine(rightSphere + Vector3.up * radius, leftSphere + Vector3.up * radius);
            Gizmos.DrawLine(rightSphere + Vector3.down * radius, leftSphere + Vector3.down * radius);
        }
    }
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
    public Vector2 FrameInput { get; }
}