using UnityEngine;

/// <summary>
/// Estado raíz que gestiona al jugador mientras está en el aire con velocidad vertical descendente.
/// Maneja la gravedad de caída, el Coyote Time y las transiciones de Dash aéreo.
/// </summary>
public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    #region State Lifecycle
    public override void EnterState()
    {
        // Permitimos que el jugador mantenga control horizontal (Idle/Run) durante la caída
        InitializeSubState();
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Limpieza opcional al aterrizar o cambiar de estado
    }
    #endregion

    #region Logic
    /// <summary>
    /// Evalúa las prioridades de transición en el aire: 1. Suelo, 2. Dash, 3. Coyote Jump.
    /// </summary>
    public override void CheckSwitchStates()
    {
        // 1. PRIORIDAD: ATERRIZAJE
        if (Context.Grounded)
        {
            SwitchState(Factory.Grounded());
            return;
        }

        // 2. PRIORIDAD: DASH AÉREO
        if (Context.DashToConsume && Context.CanDash && Context.Stats.CanDashInAir)
        {
            if (HandleDashAttempt()) return;
        }

        // 3. PRIORIDAD: COYOTE TIME (Salto justo después de caer)
        if (Context.JumpToConsume && Context.CanUseCoyote)
        {
            HandleCoyoteJumpAttempt();
        }
    }

    public override void InitializeSubState()
    {
        if (Mathf.Abs(Context.FrameInputRef.Move.x) < 0.01f)
        {
            SetSubState(Factory.Idle());
        }
        else
        {
            SetSubState(Factory.Run());
        }
    }

    /// <summary>
    /// Aplica la aceleración de caída constante hasta alcanzar la velocidad terminal definida.
    /// </summary>
    private void HandleGravity()
    {
        float speedWithGravity = Mathf.MoveTowards(
            Context.FrameVelocity.y,
            -Context.Stats.MaxFallSpeed,
            Context.Stats.FallAcceleration * Time.fixedDeltaTime
        );

        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, speedWithGravity);
    }
    #endregion

    #region Helpers (Private)
    private bool HandleDashAttempt()
    {
        if (Context.StatsController.HasEnoughStamina(Context.Stats.DashStaminaCost))
        {
            SwitchState(Factory.Dash());
            return true;
        }

        Context.DashToConsume = false;
        return false;
    }

    private void HandleCoyoteJumpAttempt()
    {
        if (Context.StatsController.HasEnoughStamina(Context.Stats.JumpStaminaCost))
        {
            SwitchState(Factory.Jump());
        }
        else
        {
            // Fallo por estamina: reseteamos flags para evitar saltos "fantasma"
            Context.JumpToConsume = false;
            Context.UseJumpBuffer();
        }
    }
    #endregion
}