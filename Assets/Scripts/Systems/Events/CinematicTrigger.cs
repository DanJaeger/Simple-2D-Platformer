using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detecta la entrada del jugador en un área específica y dispara eventos programados.
/// Ideal para activar cinemáticas, diálogos, cambios de cámara o hitos en el nivel.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CinematicTrigger : MonoBehaviour
{
    #region Serialized Fields
    [Header("Configuración de Detección")]
    [Tooltip("Capa que identifica al jugador para activar el trigger.")]
    [SerializeField] private LayerMask _playerLayer;

    [Tooltip("Si está activo, el evento solo se disparará la primera vez que se entre.")]
    [SerializeField] private bool _oneTimeOnly = true;

    [Header("Eventos de Observador")]
    [Tooltip("Evento que se invoca cuando el jugador entra en la zona. Puedes arrastrar funciones aquí desde el Inspector.")]
    public UnityEvent OnZoneEntered;
    #endregion

    #region Private Fields
    private bool _alreadyTriggered = false;
    #endregion

    #region Detection Logic
    /// <summary>
    /// Evento de Unity para detectar colisiones en 2D.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Verificación de seguridad: ¿Es la capa del jugador?
        if (IsPlayer(collision.gameObject))
        {
            // 2. Verificación de uso único
            if (_oneTimeOnly && _alreadyTriggered) return;

            ExecuteTrigger();
        }
    }

    /// <summary>
    /// Comprueba si el objeto colisionado pertenece a la capa del jugador.
    /// </summary>
    private bool IsPlayer(GameObject obj)
    {
        return ((1 << obj.layer) & _playerLayer) != 0;
    }

    /// <summary>
    /// Marca el trigger como usado e invoca los eventos asociados.
    /// </summary>
    private void ExecuteTrigger()
    {
        _alreadyTriggered = true;

        // Log informativo para depuración
        Debug.Log($"<color=cyan>CinematicTrigger:</color> Zona activada en {gameObject.name}");

        // 3. Notificación a todos los observadores (UnityEvent)
        OnZoneEntered?.Invoke();

        // 4. Limpieza: Desactiva el objeto si es de un solo uso para optimizar la escena
        if (_oneTimeOnly)
        {
            // Opcional: Podrías simplemente destruir el componente si quieres mantener el objeto visual
            gameObject.SetActive(false);
        }
    }
    #endregion

    #region Debug
    /// <summary>
    /// Dibuja visualmente el área del trigger en la ventana de Scene para facilitar el diseño de niveles.
    /// </summary>
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = _alreadyTriggered ? Color.gray : new Color(0, 1, 1, 0.3f);

        if (col is BoxCollider2D box)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
    #endregion
}