using UnityEngine;

public class CharacterStatsController : MonoBehaviour
{
    private CharacterStatsModel model;
    [SerializeField] private float initialHealth;
    [SerializeField] private float initialStamina;

    private void Awake()
    {
        model = new CharacterStatsModel(initialHealth, initialStamina);
    }

    // Método para que la Vista se suscriba al Modelo
    public CharacterStatsModel GetModel() => model;

    public void TakeDamage(float amount)
    {
        model.TakeDamage(amount); // El controlador modifica el Modelo
    }
}
