using UnityEngine;

public class CharacterStatsModel
{
    public CharacterStatsModel(float initialHealth, float initialStamina)
    {
        health = initialHealth;
        stamina = initialStamina;
    }

    // Notificication delegate
    public delegate void ValueChange(float newValue);

    #region Health Methods and Attributes
    private float health; 
    private const float c_maxHealth = 100f;
    public ValueChange OnHealthChanged;

    public float Health {
        get => health;
        set {
            health = value;
            OnHealthChanged?.Invoke(GetHealthPercentage());
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        Health = Mathf.Max(0, Health); // Evita que la salud sea negativa.
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    public float GetHealthPercentage() { return health / Mathf.Max(c_maxHealth, 0.0001f); }
    #endregion

    #region Stamina Methods and Attributes
    private float stamina;
    private const float c_maxStamina = 100f;
    public ValueChange OnStaminaChanged;

    public float Stamina
    {
        get => stamina;
        set
        {
            stamina = value;
            OnStaminaChanged?.Invoke(GetStaminaPercentage());
        }
    }
    public float GetStaminaPercentage() { return stamina / Mathf.Max(c_maxStamina, 0.0001f); }

    public void ConsumeStamina(float amount)
    {
        Stamina -= amount;
        Stamina = Mathf.Max(0, Stamina); // Evita que la salud sea negativa.
        OnStaminaChanged?.Invoke(GetStaminaPercentage());
    }
    #endregion


}
