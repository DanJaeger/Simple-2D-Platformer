using UnityEngine;
using System.Collections;

/// <summary>
/// Actúa como el Controlador (MVC) de las estadísticas del jugador.
/// Gestiona la comunicación entre el Modelo y Unity, controlando procesos temporales
/// como la regeneración de estamina mediante corrutinas.
/// </summary>
public class CharacterStatsController : MonoBehaviour
{
    #region Atributos Privados
    private CharacterStatsModel _model;
    private Coroutine _regenCoroutine;
    #endregion

    #region Configuración
    [Header("Configuración Base")]
    [Tooltip("Plantilla de datos iniciales para el modelo.")]
    [SerializeField] private CharacterStatsSO statsTemplate;
    #endregion

    private void Awake()
    {
        // Inicialización del Modelo con los datos del ScriptableObject
        _model = new CharacterStatsModel(
            statsTemplate.InitialHealth,
            statsTemplate.InitialStamina,
            statsTemplate.MaxHealth,
            statsTemplate.MaxStamina);
    }

    /// <summary>
    /// Expone el modelo para que la Vista pueda suscribirse a sus eventos.
    /// </summary>
    public CharacterStatsModel GetModel() => _model;

    #region Lógica de Interfaz de Usuario y Jugabilidad
    /// <summary>
    /// Consulta al modelo si existe estamina suficiente para una acción.
    /// </summary>
    public bool HasEnoughStamina(float cost)
    {
        return _model.HasEnoughStamina(cost);
    }

    /// <summary>
    /// Reduce la estamina en el modelo y reinicia el proceso de regeneración.
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        _model.ConsumeStamina(amount);

        // Al consumir, reiniciamos la corrutina para asegurar que la regeneración 
        // comience tras el gasto.
        RegenStamina();
    }
    #endregion

    #region Gestión de Tiempo (Corrutinas)
    /// <summary>
    /// Inicia o reinicia el hilo de regeneración de estamina.
    /// </summary>
    public void RegenStamina()
    {
        // Si ya hay una regeneración en curso, la detenemos para no solapar procesos.
        if (_regenCoroutine != null)
            StopCoroutine(_regenCoroutine);

        _regenCoroutine = StartCoroutine(RegenRoutine());
    }

    /// <summary>
    /// Hilo secundario que incrementa gradualmente la estamina en el modelo.
    /// Solo el Controlador maneja estos tiempos de espera de Unity.
    /// </summary>
    private IEnumerator RegenRoutine()
    {
        // Mientras el modelo informe que no está lleno, seguimos sumando.
        while (!_model.IsStaminaFull())
        {
            // Espera de Unity (Time-based logic)
            yield return new WaitForSeconds(statsTemplate.RecoverStaminaTime);

            // Pedimos al modelo que sume una cantidad fija.
            _model.AddStamina(statsTemplate.RecoverStaminaAmount);
        }

        // Una vez lleno, limpiamos la referencia de la corrutina.
        _regenCoroutine = null;
    }
    #endregion
}