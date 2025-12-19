using UnityEngine;

/// <summary>
/// Contenedor de datos que define todas las capacidades físicas y de recursos del jugador.
/// Permite ajustar el comportamiento del personaje desde el Inspector sin modificar el código.
/// </summary>
[CreateAssetMenu(fileName = "NuevosStatsPersonaje", menuName = "SO/Stats de Personaje")]
public class CharacterStatsSO : ScriptableObject
{
    #region Salud y Estamina
    [Header("VIDA Y ESTAMINA")]
    [Tooltip("Los puntos de vida máximos que el jugador puede alcanzar.")]
    public float MaxHealth = 100f;

    [Tooltip("Los puntos de vida con los que el jugador comienza la partida.")]
    public float InitialHealth = 100f;

    [Tooltip("La cantidad máxima de estamina disponible para acciones como Dash o Salto.")]
    public float MaxStamina = 100f;

    [Tooltip("La estamina con la que el jugador comienza la partida.")]
    public float InitialStamina = 100f;

    [Header("COSTOS DE ESTAMINA")]
    [Tooltip("Cantidad de estamina consumida cada vez que el jugador realiza un salto.")]
    public float JumpStaminaCost = 10f;

    [Tooltip("Cantidad de estamina consumida cada vez que el jugador realiza un dash.")]
    public float DashStaminaCost = 20f;

    [Header("REGENERACION DE ESTAMINA")]
    [Tooltip("Cantidad de estamina regenerada despues de cada plazo de tiempo.")]
    public float RecoverStaminaAmount = 1f;

    [Tooltip("Plazo de tiempo para recarga por punto de 'RecoverStaminaAmount'.")]
    public float RecoverStaminaTime = 0.3f;
    #endregion

    #region Capas de Física
    [Header("CAPAS (LAYERS)")]
    [Tooltip("La LayerMask usada para identificar al jugador. Se usa para excluir el propio collider del jugador en chequeos físicos.")]
    public LayerMask PlayerLayer;
    #endregion

    #region Configuración de Input
    [Header("INPUT")]
    [Tooltip("Si está activo, el input de movimiento se ajustará a -1, 0 o 1. Útil para un comportamiento consistente entre teclado y mandos analógicos.")]
    public bool SnapInput = true;

    [Tooltip("Input vertical mínimo requerido para registrar una acción (trepar, etc). Previene inputs accidentales por el drift de la palanca."), Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f;

    [Tooltip("Input horizontal mínimo requerido para registrar movimiento. Previene que el personaje se deslice solo por drift del hardware."), Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;
    #endregion

    #region Ajustes de Movimiento
    [Header("MOVIMIENTO")]
    [Tooltip("La velocidad horizontal máxima que el jugador puede alcanzar.")]
    public float MaxSpeed = 14f;

    [Tooltip("Qué tan rápido el jugador alcanza la Velocidad Máxima. Valores altos resultan en un movimiento más reactivo.")]
    public float Acceleration = 120f;

    [Tooltip("Qué tan rápido se detiene el jugador al tocar el suelo después de soltar el input.")]
    public float GroundDeceleration = 60f;

    [Tooltip("Qué tan rápido se detiene el jugador mientras está en el aire después de soltar el input.")]
    public float AirDeceleration = 30f;

    [Tooltip("Fuerza descendente constante aplicada mientras está en el suelo para asegurar que el jugador se pegue a pendientes y terreno irregular."), Range(0f, -10f)]
    public float GroundingForce = -1.5f;

    [Tooltip("La distancia que el sistema de física chequea por debajo y por encima del collider para detectar suelo o techos."), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;
    #endregion

    #region Ajustes de Salto
    [Header("SALTO")]
    [Tooltip("La velocidad inicial ascendente aplicada en el momento de saltar.")]
    public float JumpPower = 18f;

    [Tooltip("Velocidad terminal o velocidad máxima a la que el jugador puede caer.")]
    public float MaxFallSpeed = 40f;

    [Tooltip("La tasa a la que el jugador gana velocidad descendente mientras está en el aire (Gravedad).")]
    public float FallAcceleration = 50f;

    [Tooltip("Multiplicador de gravedad aplicado cuando se suelta el botón de salto antes de alcanzar el punto máximo. Permite saltos de altura variable.")]
    public float JumpEndEarlyGravityModifier = 3f;

    [Tooltip("Periodo de gracia (en segundos) que permite al jugador saltar justo después de caminar fuera de una plataforma.")]
    public float CoyoteTime = 0.15f;

    [Tooltip("Tiempo (en segundos) que el juego recuerda un input de salto antes de tocar el suelo. Hace que los controles se sientan más fluidos.")]
    public float JumpBuffer = 0.2f;
    #endregion

    #region Ajustes de Dash
    [Header("DASH")]
    [Tooltip("La velocidad instantánea aplicada al jugador durante un dash.")]
    public float DashPower = 20f;

    [Tooltip("La duración (en segundos) que dura el dash, durante la cual la gravedad suele ignorarse.")]
    public float DashDuration = 0.2f;

    [Tooltip("Periodo de enfriamiento (en segundos) antes de que el dash pueda usarse de nuevo.")]
    public float DashCooldown = 0.5f;

    [Tooltip("Si está activo, el jugador puede realizar un dash mientras no está en el suelo (estilo Celeste/Ori).")]
    public bool CanDashInAir = true;
    #endregion
}