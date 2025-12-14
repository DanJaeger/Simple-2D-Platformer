using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatsView : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    private CharacterStatsModel model;

    private void Start()
    {
        // 1. Obtener el Modelo a través del Controller
        CharacterStatsController controller = GetComponent<CharacterStatsController>();
        model = controller.GetModel();

        // 2. Suscribirse al evento del Modelo
        model.OnHealthChanged += UpdateHealthBar;
        model.OnStaminaChanged += UpdateStaminaBar;

        // Inicializar la barra con el valor actual
        UpdateHealthBar(model.GetHealthPercentage());
        UpdateStaminaBar(model.GetStaminaPercentage());

    }

    private void UpdateStaminaBar(float newValue)
    {
        if (staminaSlider != null)
        {
            // La Vista solo lee el valor y actualiza la UI
            staminaSlider.value = newValue;
            Debug.Log($"UI Actualizada: {newValue}");
        }
    }

    private void UpdateHealthBar(float newHealth)
    {
        if (healthSlider != null)
        {
            // La Vista solo lee el valor y actualiza la UI
            healthSlider.value = newHealth;
            Debug.Log($"UI Actualizada: {newHealth}");
        }
    }
    void OnDestroy()
    {
        // Importante: Desuscribirse para evitar errores
        if (model != null)
        {
            model.OnHealthChanged -= UpdateHealthBar;
            model.OnStaminaChanged -= UpdateStaminaBar;
        }
    }
}
