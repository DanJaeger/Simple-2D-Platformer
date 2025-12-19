using UnityEngine;

/// <summary>
/// Sub-estado que gestiona al jugador cuando no hay input de movimiento horizontal.
/// Se encarga de reducir la velocidad actual hasta detener al personaje por completo.
/// </summary>
public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    #region State Lifecycle
    public override void EnterState()
    {
        // Debug.Log("Entrando en modo: IDLE");
    }

    public override void UpdateState()
    {
        ApplyDeceleration();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Limpieza opcional
    }
    #endregion

    #region Logic
    /// <summary>
    /// Cambia al estado de carrera (Run) si el input horizontal supera el umbral muerto.
    /// </summary>
    public override void CheckSwitchStates()
    {
        if (Mathf.Abs(Context.FrameInputRef.Move.x) > 0.01f)
        {
            SwitchState(Factory.Run());
        }
    }

    public override void InitializeSubState()
    {
        // Idle es un estado hoja (leaf state), no suele tener sub-estados.
    }

    /// <summary>
    /// Aplica una fuerza de frenado gradual basada en los stats de fricción.
    /// </summary>
    private void ApplyDeceleration()
    {
        // Seleccionamos la fricción adecuada según el contexto físico
        float deceleration = Context.Grounded
            ? Context.Stats.GroundDeceleration
            : Context.Stats.AirDeceleration;

        // Calculamos la nueva velocidad X aproximándola a cero de forma suave
        float newX = Mathf.MoveTowards(
            Context.FrameVelocity.x,
            0,
            deceleration * Time.fixedDeltaTime
        );

        Context.FrameVelocity = new Vector2(newX, Context.FrameVelocity.y);
    }
    #endregion
}