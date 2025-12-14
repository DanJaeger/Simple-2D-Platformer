using UnityEngine;

public class CharacterStatsController : MonoBehaviour
{
    private CharacterStatsModel model;
    [SerializeField] private CharacterStatsSO statsTemplate;

    private void Awake()
    {
        model = new CharacterStatsModel(
            statsTemplate.initialHealth,
            statsTemplate.initialStamina,
            statsTemplate.maxHealth,
            statsTemplate.maxStamina);
    }

    // Método para que la Vista se suscriba al Modelo
    public CharacterStatsModel GetModel() => model;

    public void TakeDamage(float amount)
    {
        model.TakeDamage(amount); // El controlador modifica el Modelo
    }
}
