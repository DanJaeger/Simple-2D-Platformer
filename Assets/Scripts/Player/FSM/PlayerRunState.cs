using UnityEngine;

/// <summary>
/// Sub-estado que gestiona el movimiento horizontal activo del jugador.
/// Se encarga de acelerar al personaje hacia la velocidad máxima definida en los stats.
/// </summary>
public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    #region State Lifecycle
    public override void EnterState()
    {
        // Debug.Log("Entrando en modo: RUN");
    }

    public override void UpdateState()
    {
        HandleHorizontalMovement();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Limpieza opcional
    }
    #endregion

    #region Logic
    /// <summary>
    /// Regresa al estado Idle si el input horizontal es nulo o insignificante.
    /// </summary>
    public override void CheckSwitchStates()
    {
        if (Mathf.Abs(Context.FrameInputRef.Move.x) < 0.01f)
        {
            SwitchState(Factory.Idle());
        }
    }

    public override void InitializeSubState()
    {
        // Run es un estado hoja (leaf state) dentro de la jerarquía actual.
    }

    /// <summary>
    /// Calcula y aplica la aceleración horizontal basada en el input del jugador.
    /// </summary>
    private void HandleHorizontalMovement()
    {
        // Calculamos la velocidad objetivo basada en la dirección del input
        float targetSpeed = Context.FrameInputRef.Move.x * Context.Stats.MaxSpeed;

        // Suavizamos el cambio de velocidad actual hacia la velocidad objetivo
        float newX = Mathf.MoveTowards(
            Context.FrameVelocity.x,
            targetSpeed,
            Context.Stats.Acceleration * Time.fixedDeltaTime
        );

        // Actualizamos la velocidad en el contexto sin afectar el eje Y (gravedad)
        Context.FrameVelocity = new Vector2(newX, Context.FrameVelocity.y);
    }
    #endregion
}