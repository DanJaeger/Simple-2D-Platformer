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

    [Header("References")]
    [SerializeField, Tooltip("Referencia explícita al Rigidbody2D (opcional, se obtiene en Reset si está vacío).")] private Rigidbody2D rb;

    // Estado
    private Vector2 currentVelocity = Vector2.zero; // velocidad almacenada (en unidades/segundo)
    private Vector2 input = Vector2.zero;

    // Cachés
    private float maxAccSq;

    //Flags
    bool mStarted = false;

    private void Reset()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Para evitar que Unity aplique rotación física (si vas a rotar por transform), congelamos rotación.
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            damping = rb.linearDamping;
        }
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            damping = rb.linearDamping;
        }
        if (mainCamera == null) mainCamera = Camera.main;
        maxAccSq = maxAcceleration * maxAcceleration;
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.Interact && !mStarted) { 
            mStarted = true;
            GetComponent<CharacterStatsController>().TakeDamage(10f);
        }

        // --- Leer input en Update (más responsivo) ---
        input = InputManager.Instance.PlayerMovementInput;
        if (input.sqrMagnitude > 1f) input = input.normalized;
        
    }

    private void FixedUpdate()
    {
        // --- Physics update ---
        // 1) Calculamos la velocidad deseada a partir del input
        Vector2 desiredVelocity = input * maxSpeed;

        // 2) Calculamos la aceleración necesaria usando un timeToTarget estable
        Vector2 neededAcceleration = (desiredVelocity - currentVelocity) / Mathf.Max(0.0001f, timeToReachNeededVelocity);

        // 3) Clamp usando sqrMagnitude para ahorrar sqrt
        if (neededAcceleration.sqrMagnitude > maxAccSq)
        {
            neededAcceleration = neededAcceleration.normalized * maxAcceleration;
        }

        // 4) Integración semi-implícita (más estable que la explícita) para la velocidad
        //    currentVelocity = currentVelocity + accel * dt
        float dt = Time.fixedDeltaTime;
        currentVelocity += neededAcceleration * dt;

        // 5) Damping exponencial cuando no hay input (usa dt físico)
        if (input.sqrMagnitude < 0.0001f)
        {
            float alpha = 1f - Mathf.Exp(-damping * dt);
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, alpha);
        }

        // 6) Aplicar velocidad al Rigidbody2D (más correcto que MovePosition si quieres interacciones físicas)
        if (rb != null)
        {
            //rb.linearVelocity = currentVelocity;
            Vector2 velocityError = new Vector2(currentVelocity.x, currentVelocity.y) - rb.linearVelocity;
            Vector2 requiredAcceleration = velocityError / Mathf.Max(0.000001f, dt);
            Vector2 force = requiredAcceleration * rb.mass;
            rb.AddForce(force, ForceMode2D.Force);
        }
        else
        {
            // Fallback (si no hay Rigidbody) — mover el transform manualmente
            transform.position += (Vector3)(currentVelocity * dt);
        }
    }

    // Métodos de ayuda públicos
    public Vector2 GetVelocity() => currentVelocity;
    public Vector2 GetForward() => transform.right;
}
