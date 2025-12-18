using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "SO/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    #region Health and Stamina
    [Header("HEALTH & STAMINA")]
    [Tooltip("The maximum health points the player can reach.")]
    public float MaxHealth = 100f;

    [Tooltip("The health points the player starts with when the game begins.")]
    public float InitialHealth = 100f;

    [Tooltip("The maximum stamina points available for actions like dashing or jumping.")]
    public float MaxStamina = 100f;

    [Tooltip("The stamina points the player starts with when the game begins.")]
    public float InitialStamina = 100f;

    [Header("STAMINA COSTS")]
    [Tooltip("The amount of stamina consumed each time the player performs a jump.")]
    public float JumpStaminaCost = 10f;

    [Tooltip("The amount of stamina consumed each time the player performs a dash.")]
    public float DashStaminaCost = 20f;
    #endregion

    #region Physics Layers
    [Header("LAYERS")]
    [Tooltip("The LayerMask used to identify the player. Used to exclude the player's own collider from physics checks.")]
    public LayerMask PlayerLayer;
    #endregion

    #region Input Settings
    [Header("INPUT")]
    [Tooltip("If enabled, movement input will snap to -1, 0, or 1. Useful for consistent behavior between keyboard and analog controllers.")]
    public bool SnapInput = true;

    [Tooltip("Minimum vertical input required to register an action (climbing, etc). Prevents accidental inputs from stick drift."), Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f;

    [Tooltip("Minimum horizontal input required to register movement. Prevents character drifting due to hardware stick drift."), Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;
    #endregion

    #region Movement Settings
    [Header("MOVEMENT")]
    [Tooltip("The maximum horizontal velocity the player can achieve.")]
    public float MaxSpeed = 14f;

    [Tooltip("How quickly the player reaches Max Speed. High values result in snappier movement.")]
    public float Acceleration = 120f;

    [Tooltip("How quickly the player slows down while touching the ground after releasing input.")]
    public float GroundDeceleration = 60f;

    [Tooltip("How quickly the player slows down while in the air after releasing input.")]
    public float AirDeceleration = 30f;

    [Tooltip("A constant downward force applied while grounded to ensure the player sticks to slopes and uneven terrain."), Range(0f, -10f)]
    public float GroundingForce = -1.5f;

    [Tooltip("The distance the physics system checks below and above the collider to detect ground or ceilings."), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;
    #endregion

    #region Jump Settings
    [Header("JUMP")]
    [Tooltip("The initial upward velocity applied at the moment of jumping.")]
    public float JumpPower = 18f;

    [Tooltip("The terminal velocity or maximum speed at which the player can fall.")]
    public float MaxFallSpeed = 40f;

    [Tooltip("The rate at which the player gains downward velocity while in the air (Gravity).")]
    public float FallAcceleration = 50f;

    [Tooltip("The gravity multiplier applied when the jump button is released before reaching the apex. Allows for variable jump heights.")]
    public float JumpEndEarlyGravityModifier = 3f;

    [Tooltip("The grace period (in seconds) that allows a player to jump after walking off a ledge.")]
    public float CoyoteTime = 0.15f;

    [Tooltip("How long (in seconds) the game remembers a jump input before hitting the ground. Makes the controls feel more responsive.")]
    public float JumpBuffer = 0.2f;
    #endregion

    #region Dash Settings
    [Header("DASH")]
    [Tooltip("The instantaneous velocity applied to the player during a dash.")]
    public float DashPower = 20f;

    [Tooltip("The duration (in seconds) that the dash lasts, during which gravity is typically ignored.")]
    public float DashDuration = 0.2f;

    [Tooltip("The cooldown period (in seconds) before the dash can be used again.")]
    public float DashCooldown = 0.5f;

    [Tooltip("If enabled, the player can perform a dash while not grounded (Ori/Celeste style).")]
    public bool CanDashInAir = true;
    #endregion
}