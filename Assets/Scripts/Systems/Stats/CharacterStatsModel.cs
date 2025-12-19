using System;

/// <summary>
/// Representa los datos puros y las reglas de negocio de los atributos del jugador.
/// Esta clase es independiente de Unity (Model), encargándose únicamente del 
/// almacenamiento, validación (clamping) y notificación de cambios en vida y estamina.
/// </summary>
public class CharacterStatsModel
{
    #region Atributos Privados
    private float _health;
    private float _stamina;
    private readonly float _maxHealth;
    private readonly float _maxStamina;
    #endregion

    #region Eventos (Observer)
    /// <summary> Se dispara cuando la vida cambia, enviando el porcentaje actual (0 a 1). </summary>
    public Action<float> OnHealthChanged;

    /// <summary> Se dispara cuando la estamina cambia, enviando el porcentaje actual (0 a 1). </summary>
    public Action<float> OnStaminaChanged;
    #endregion

    #region Propiedades Públicas
    public float Health => _health;
    public float Stamina => _stamina;
    #endregion

    /// <summary>
    /// Constructor del modelo. Inicializa valores y aplica límites de seguridad.
    /// </summary>
    public CharacterStatsModel(float initHealth, float initStamina, float maxH, float maxS)
    {
        _maxHealth = maxH;
        _maxStamina = maxS;

        // Usamos los métodos Set para asegurar que el valor inicial esté dentro de los límites
        SetHealth(initHealth);
        SetStamina(initStamina);
    }

    #region Consultas de Estado
    /// <summary> Devuelve el porcentaje de vida actual para uso en barras de interfaz. </summary>
    public float GetHealthPercentage() => _health / Math.Max(_maxHealth, 0.0001f);

    /// <summary> Devuelve el porcentaje de estamina actual para uso en barras de interfaz. </summary>
    public float GetStaminaPercentage() => _stamina / Math.Max(_maxStamina, 0.0001f);

    /// <summary> Comprueba si la estamina ha llegado a su límite máximo. </summary>
    public bool IsStaminaFull() => _stamina >= _maxStamina;

    /// <summary>
    /// Verifica si el jugador dispone de la estamina suficiente para una acción.
    /// </summary>
    /// <param name="cost">Cantidad a descontar.</param>
    public bool HasEnoughStamina(float cost) => _stamina >= cost;
    #endregion

    #region Métodos de Modificación (Lógica de Negocio)
    /// <summary>
    /// Actualiza la vida aplicando límites y notificando a los suscriptores si hubo cambios.
    /// </summary>
    public void SetHealth(float value)
    {
        float clamped = Math.Clamp(value, 0, _maxHealth);
        // Solo notificamos si el cambio es significativo para ahorrar procesamiento
        if (Math.Abs(_health - clamped) > 0.001f)
        {
            _health = clamped;
            OnHealthChanged?.Invoke(GetHealthPercentage());
        }
    }

    /// <summary>
    /// Actualiza la estamina aplicando límites y notificando a los suscriptores si hubo cambios.
    /// </summary>
    public void SetStamina(float value)
    {
        float clamped = Math.Clamp(value, 0, _maxStamina);
        if (Math.Abs(_stamina - clamped) > 0.001f)
        {
            _stamina = clamped;
            OnStaminaChanged?.Invoke(GetStaminaPercentage());
        }
    }

    /// <summary> Reduce la vida actual en una cantidad específica. </summary>
    public void TakeDamage(float amount) => SetHealth(_health - amount);

    /// <summary> Reduce la estamina actual en una cantidad específica. </summary>
    public void ConsumeStamina(float amount) => SetStamina(_stamina - amount);

    /// <summary> Incrementa la estamina actual (Usado por el Controlador en procesos de regeneración). </summary>
    public void AddStamina(float amount) => SetStamina(_stamina + amount);
    #endregion
}