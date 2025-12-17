using UnityEngine;
using UnityEngine.Events; // Necesario para el "Observer" de Unity

public class CinematicTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private bool _oneTimeOnly = true;

    [Header("Observer Event")]
    // Este es nuestro "Grito". En el inspector verás un cuadro para arrastrar objetos.
    public UnityEvent OnZoneEntered;

    private bool _alreadyTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Verificar si es el jugador usando la Layer
        if (((1 << collision.gameObject.layer) & _playerLayer) != 0)
        {
            if (_oneTimeOnly && _alreadyTriggered) return;

            ExecuteTrigger();
        }
    }

    private void ExecuteTrigger()
    {
        _alreadyTriggered = true;

        // 2. Notificar a todos los observadores
        Debug.Log($"Zona de cinemática activada: {gameObject.name}");
        OnZoneEntered?.Invoke();

        // Opcional: Desactivar el objeto si ya no se necesita
        if (_oneTimeOnly) gameObject.SetActive(false);
    }
}