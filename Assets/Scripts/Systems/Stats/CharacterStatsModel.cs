using System;

public class CharacterStatsModel
{
    public CharacterStatsModel(float initialHealth, float initialStamina, float initialMaxHealth, float initialMaxStamina)
    {
        health = initialHealth;
        stamina = initialStamina;
        c_maxHealth = initialMaxHealth;
        c_maxStamina = initialMaxStamina;
    }

    // Notificication delegate
    public delegate void ValueChange(float newValue);

    #region Health Methods and Attributes
    private float health;
    private readonly float c_maxHealth;
    public ValueChange OnHealthChanged;

    public float Health {
        get => health;
        set {
            // 1. Aplicar límite (Clamping)
            float clampedValue = Math.Max(0, Math.Min(c_maxHealth, value));

            // 2. Solo notificar si el valor ha cambiado
            if (health != clampedValue)
            {
                health = clampedValue;
                OnHealthChanged?.Invoke(GetHealthPercentage());
            }
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    public float GetHealthPercentage() { return health / Math.Max(c_maxHealth, 0.0001f); }
    #endregion

    #region Stamina Methods and Attributes
    private float stamina;
    private readonly float c_maxStamina;
    public ValueChange OnStaminaChanged;

    public float Stamina
    {
        get => stamina;
        set
        {
            // 1. Aplicar límite (Clamping)
            float clampedValue = Math.Max(0, Math.Min(c_maxStamina, value));

            // 2. Solo notificar si el valor ha cambiado
            if (stamina != clampedValue)
            {
                stamina = clampedValue;
                OnStaminaChanged?.Invoke(GetStaminaPercentage());
            }
        }
    }
    public float GetStaminaPercentage() { return stamina / Math.Max(c_maxStamina, 0.0001f); }

    public void ConsumeStamina(float amount)
    {
        Stamina -= amount;
    }
    #endregion


}
