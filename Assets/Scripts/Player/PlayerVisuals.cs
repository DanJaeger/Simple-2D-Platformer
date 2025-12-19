using UnityEngine;

/// <summary>
/// Gestiona la representación visual del jugador, incluyendo animaciones y orientación del sprite.
/// Escucha eventos de PlayerStateMachine para reaccionar a acciones únicas (Salto, Dash).
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;
    #endregion

    #region Private Fields
    private PlayerStateMachine _player;
    private Rigidbody2D _playerRB;
    private float _baseScaleX;

    // Hashes de parámetros: Optimización para evitar buscar strings en el Animator cada frame
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");
    private static readonly int VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int DashTrigger = Animator.StringToHash("Dash");
    #endregion

    #region Lifecycle
    private void Awake()
    {
        _player = GetComponent<PlayerStateMachine>();
        _playerRB = GetComponent<Rigidbody2D>();

        // Cacheamos la escala inicial definida en el inspector para evitar "Hardcoding"
        _baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void OnEnable()
    {
        // Suscripción a eventos (Patrón Observer)
        _player.Jumped += OnJump;
        _player.Dashed += OnDash;
        _player.GroundedChanged += OnGroundedChanged;
    }

    private void OnDisable()
    {
        // Desuscripción obligatoria para evitar fugas de memoria (Memory Leaks)
        _player.Jumped -= OnJump;
        _player.Dashed -= OnDash;
        _player.GroundedChanged -= OnGroundedChanged;
    }

    private void Update()
    {
        HandleSpriteFlip();
        UpdateAnimatorParameters();
    }
    #endregion

    #region Visual Logic
    /// <summary>
    /// Gira el transform del jugador basándose en la dirección del movimiento.
    /// </summary>
    private void HandleSpriteFlip()
    {
        // Solo giramos si el jugador está enviando input de movimiento significativo
        if (Mathf.Abs(_player.FrameInputRef.Move.x) > 0.01f)
        {
            float direction = Mathf.Sign(_player.FrameInputRef.Move.x);

            // Aplicamos la escala manteniendo el valor base (ej: 3.5f)
            transform.localScale = new Vector3(direction * _baseScaleX, transform.localScale.y, 1);
        }
    }

    /// <summary>
    /// Sincroniza las variables del Animator con el estado físico actual.
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        // VelocityX: Se usa para transición Idle -> Walk/Run
        _anim.SetFloat(VelocityX, Mathf.Abs(_player.FrameInputRef.Move.x));

        // VelocityY: Se usa para determinar si subimos o caemos en las animaciones de aire
        _anim.SetFloat(VelocityY, _playerRB.linearVelocity.y);
    }
    #endregion

    #region Event Handlers
    private void OnJump() => _anim.SetTrigger(JumpTrigger);

    private void OnDash() => _anim.SetTrigger(DashTrigger);

    private void OnGroundedChanged(bool isGrounded, float impactVelocity)
    {
        _anim.SetBool(Grounded, isGrounded);

        // Aquí podrías disparar efectos de partículas de aterrizaje usando impactVelocity
    }
    #endregion
}