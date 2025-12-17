using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;
    private PlayerController _player;
    private Rigidbody2D _playerRB;

    // Hashes de parámetros (Más eficiente que usar Strings cada frame)
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");
    private static readonly int VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _playerRB = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        // Nos suscribimos a los eventos del controlador
        _player.Jumped += OnJump;
        _player.GroundedChanged += OnGroundedChanged;
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJump;
        _player.GroundedChanged -= OnGroundedChanged;
    }

    private void Update()
    {
        // 1. Flip del Sprite basado en el input
        // Solo giramos si el input es distinto de cero
        if (Mathf.Abs(_player.FrameInput.x) > 0.01f)
        {
            // Creamos un nuevo vector de escala basado en el signo del input
            // Mathf.Sign devuelve 1 para positivo y -1 para negativo
            float direction = Mathf.Sign(_player.FrameInput.x);

            transform.localScale = new Vector3(direction * 3.5f, 3.5f, 1);
        }

        // 2. Actualizar parámetros continuos
        // Usamos Mathf.Abs para la animación de correr
        _anim.SetFloat(VelocityX, Mathf.Abs(_player.FrameInput.x));

        // Enviamos la velocidad real del Rigidbody para animaciones de caída/salto
        _anim.SetFloat(VelocityY, _playerRB.linearVelocity.y);
    }

    private void OnJump()
    {
        _anim.SetTrigger(JumpTrigger);
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _anim.SetBool(Grounded, grounded);
    }
}