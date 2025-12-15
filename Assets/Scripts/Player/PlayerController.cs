using System;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement (physics)")]
    [SerializeField, Tooltip("Velocidad máxima en unidades/segundo.")] private float maxSpeed = 20f;
    [SerializeField, Tooltip("Aceleración máxima en unidades/segundo^2.")] private float maxAcceleration = 6f;
    [SerializeField, Tooltip("Coeficiente de damping (amortiguamiento) usado para frenar cuando no hay input.")] private float damping = 5f;
    [SerializeField, Tooltip("Tiempo objetivo para acercarse a la velocidad deseada (segundos). Usa esto en vez de depender del dt directamente.")] private float timeToReachNeededVelocity = 0.5f;

    [Header("Rotation")]
    [SerializeField, Tooltip("Girar hacia el raton (true) o no (false). Si usas Rigidbody2D habilita FreezeRotation en el inspector o por código para evitar conflictos.")] private bool rotateToMouse = false;
    [SerializeField, Tooltip("Referencia a la cámara principal (opcional, por defecto Camera.main en Reset).")] private Camera mainCamera;

    [Header("Jump (physics)")]
    [SerializeField, Tooltip("Altura máxima de salto deseada (en unidades).")]
    private float jumpHeight = 4f;
    [SerializeField, Tooltip("Factor extra para la gravedad cuando el personaje está cayendo (factor > 1).")]
    private float fallGravityMultiplier = 3f; // Valor recomendado: 3x a 5x
    [SerializeField, Tooltip("Layer(s) que identifican el suelo.")]
    private LayerMask groundLayer;
    [SerializeField, Tooltip("Radio de la esfera para chequear el suelo.")]
    private float groundCheckRadius = 0.2f;
    [SerializeField, Tooltip("Transform que indica la posición del check de suelo. **¡Asignación Obligatoria!**")]
    private Transform groundCheckPoint;

    [Header("References")]
    [SerializeField, Tooltip("Referencia explícita al Rigidbody2D (opcional, se obtiene en Reset si está vacío).")] private Rigidbody2D rb;

    // Estado
    private Vector2 currentVelocity = Vector2.zero;
    private Vector2 input = Vector2.zero;
    private bool isGrounded = false;

    // Cachés
    private float maxAccSq;
    private float jumpImpulseMagnitude; // Magnitud del impulso (masa * velocidad inicial)

    //Flags
    bool mStarted = false;

    private void Reset()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        if (mainCamera == null) mainCamera = Camera.main;
        maxAccSq = maxAcceleration * maxAcceleration;

        // CÁLCULO DE LA MAGNITUD DEL IMPULSO DE SALTO (F_impulso = m * v0)
        // La velocidad de despegue (v0) es: v0 = sqrt(2 * g * h)
        float gravityScale = (rb != null ? rb.gravityScale : 1f);
        float gravityMagnitude = Mathf.Abs(Physics2D.gravity.y * gravityScale);

        // Evitar divisiones por cero si la gravedad es 0
        if (gravityMagnitude <= 0.0001f) gravityMagnitude = 9.81f;

        float jumpSpeed = Mathf.Sqrt(2f * gravityMagnitude * jumpHeight);

        // Impulso (masa * delta_v)
        jumpImpulseMagnitude = (rb != null ? rb.mass : 1f) * jumpSpeed;
    }

    void Update()
    {
        if (InputManager.Instance.Interact && !mStarted)
        {
            mStarted = true;
            GetComponent<CharacterStatsController>().TakeDamage(10f);
        }

        // --- Leer input en Update (más responsivo) ---
        input = InputManager.Instance.PlayerMovementInput;
        if (input.sqrMagnitude > 1f) input = input.normalized;

        // --- Lógica de Salto (se activa con el input de salto) ---
        if (InputManager.Instance.Jump)
        {
            if (isGrounded)
            {
                Jump();
            }
            // Consumimos el input de salto (depende de cómo esté implementado InputManager)
            // Asumo que tu InputManager resetea "Jump" después de ser leído.
        }

        if (rotateToMouse && mainCamera != null)
        {
            RotateToMouse();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // 0) Chequear si está en el suelo (Lógica de GroundCheck)
        CheckIfGrounded();

        // --- APLICACIÓN DE GRAVEDAD VARIABLE ---
        ApplyVariableGravity();

        // --- CÁLCULO DE MOVIMIENTO LATERAL ---

        // **CORRECCIÓN 2: IGNORAR EL MOVIMIENTO VERTICAL DE INPUT (W/S) **
        // Solo usamos la componente X (Horizontal) para el movimiento del personaje.
        Vector2 movementInput = new Vector2(input.x, 0f);

        // 1) Calculamos la velocidad deseada a partir del input
        Vector2 desiredVelocity = movementInput * maxSpeed;

        // 2) Calculamos la aceleración necesaria usando el error de velocidad.
        Vector2 velocityError = desiredVelocity - currentVelocity;
        Vector2 neededAcceleration = velocityError / Mathf.Max(0.0001f, timeToReachNeededVelocity);

        // 3) Clamp
        if (neededAcceleration.sqrMagnitude > maxAccSq)
        {
            neededAcceleration = neededAcceleration.normalized * maxAcceleration;
        }

        // 4) Integración de la velocidad *manualmente calculada*
        float dt = Time.fixedDeltaTime;
        currentVelocity += neededAcceleration * dt;

        // 5) Damping (amortiguamiento) para la velocidad calculada
        if (movementInput.sqrMagnitude < 0.0001f)
        {
            float alpha = 1f - Mathf.Exp(-damping * dt);
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, alpha);
        }

        // 6) APLICAR FUERZA DE CORRECCIÓN
        // La fuerza corrige el error entre currentVelocity (deseada) y rb.linearVelocity (actual).
        // **IMPORTANTE**: Solo corregimos la velocidad en el eje X. La velocidad Y es manejada por gravedad/salto.
        Vector2 correctionVelocityError = new Vector2(currentVelocity.x, rb.linearVelocity.y) - rb.linearVelocity;

        Vector2 requiredCorrectionAcceleration = correctionVelocityError / Mathf.Max(0.000001f, dt);
        Vector2 force = requiredCorrectionAcceleration * rb.mass;

        rb.AddForce(force, ForceMode2D.Force);
    }

    private void ApplyVariableGravity()
    {
        // 1. Aplicar Gravedad Acelerada cuando Cae (rb.linearVelocity.y < 0)
        if (rb.linearVelocity.y < 0 && !isGrounded)
        {
            // Calculamos la fuerza de gravedad extra: F = g * m * (multiplier - 1)
            // Se resta 1 porque la gravedad estándar (rb.gravityScale) ya se aplica.

            float gravityScale = rb.gravityScale;

            // Usamos la gravedad global del proyecto
            Vector2 extraGravity = Physics2D.gravity * gravityScale * (fallGravityMultiplier - 1) * rb.mass;

            // Aplicamos la fuerza de forma continua (Force)
            rb.AddForce(extraGravity, ForceMode2D.Force);
        }

        // 2. Opcional: Aplicar un pequeño 'salto más corto' cuando se suelta el botón (Si fuera necesario)
        // (Esto no lo implementaremos a menos que lo pidas, ya que complica la lógica.)
    }

    // --- MÉTODOS DE SALTO Y SUELO ---

    private void CheckIfGrounded()
    {
        if (groundCheckPoint == null)
        {
            Debug.LogError("GroundCheckPoint no está asignado. ¡Es necesario para el salto!");
            isGrounded = false;
            return;
        }

        // Chequeo de colisión de esfera.
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer) != null;
    }

    private void Jump()
    {
        // Cancelar cualquier velocidad vertical descendente.
        // Opcional, pero ayuda a que la altura del salto sea consistente.
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        // **CORRECCIÓN 3: Uso de AddForce con Impulse**
        // Impulse aplica la fuerza instantánea F = m * delta_v, lo que garantiza la velocidad de despegue calculada.
        rb.AddForce(Vector2.up * jumpImpulseMagnitude, ForceMode2D.Impulse);

        // Marcamos como no grounded inmediatamente.
        isGrounded = false;
    }

    private void RotateToMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
    }


    // Métodos de ayuda públicos
    public Vector2 GetVelocity() => currentVelocity;
    public Vector2 GetForward() => transform.right;
}