using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "SO/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    public float maxHealth = 100f;
    public float initialHealth = 100f;
    public float maxStamina = 100f;
    public float initialStamina = 100f;
}