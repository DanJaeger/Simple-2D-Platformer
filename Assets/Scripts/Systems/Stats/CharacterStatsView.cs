using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la interfaz de usuario (Vista) de las estadísticas del jugador.
/// Su única responsabilidad es suscribirse a los cambios del Modelo y 
/// actualizar los elementos visuales (Sliders) en consecuencia.
/// </summary>
public class CharacterStatsView : MonoBehaviour
{
    #region Referencias UI
    [Header("Elementos de Interfaz")]
    [Tooltip("Barra de UI que representa la salud del jugador.")]
    [SerializeField] private Slider healthSlider;

    [Tooltip("Barra de UI que representa la estamina del jugador.")]
    [SerializeField] private Slider staminaSlider;
    #endregion

    #region Atributos Privados
    private CharacterStatsModel _model;
    #endregion

    #region Ciclo de Vida
    private void Start()
    {
        // 1. Obtener el Modelo a través del Controlador. 
        // Nota: Asume que el Controlador está en el mismo GameObject.
        CharacterStatsController controller = GetComponent<CharacterStatsController>();

        if (controller == null)
        {
            Debug.LogError($"<color=red>Error:</color> No se encontró CharacterStatsController en {gameObject.name}");
            return;
        }

        _model = controller.GetModel();

        // 2. Suscripción a eventos del Modelo (Patrón Observer)
        _model.OnHealthChanged += UpdateHealthBar;
        _model.OnStaminaChanged += UpdateStaminaBar;

        // 3. Inicialización: Ponemos las barras en su estado actual al iniciar el juego
        UpdateHealthBar(_model.GetHealthPercentage());
        UpdateStaminaBar(_model.GetStaminaPercentage());
    }

    /// <summary>
    /// Es vital desvincular los eventos para evitar fugas de memoria y errores
    /// cuando el objeto es destruido o se cambia de escena.
    /// </summary>
    private void OnDestroy()
    {
        if (_model != null)
        {
            _model.OnHealthChanged -= UpdateHealthBar;
            _model.OnStaminaChanged -= UpdateStaminaBar;
        }
    }
    #endregion

    #region Métodos de Actualización Visual
    /// <summary>
    /// Actualiza el slider de estamina con el nuevo valor normalizado (0 a 1).
    /// </summary>
    private void UpdateStaminaBar(float normalizedValue)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = normalizedValue;
            // Debug.Log($"UI Estamina: {normalizedValue * 100}%");
        }
    }

    /// <summary>
    /// Actualiza el slider de salud con el nuevo valor normalizado (0 a 1).
    /// </summary>
    private void UpdateHealthBar(float normalizedValue)
    {
        if (healthSlider != null)
        {
            healthSlider.value = normalizedValue;
            // Debug.Log($"UI Salud: {normalizedValue * 100}%");
        }
    }
    #endregion
}