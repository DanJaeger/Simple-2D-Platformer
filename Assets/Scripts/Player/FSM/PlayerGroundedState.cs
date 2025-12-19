using UnityEngine;

/// <summary>
/// Estado raíz que gestiona al jugador mientras está en contacto con el suelo.
/// Controla la transición a saltos, dashes y caídas, además de mantener al jugador pegado a la superficie.
/// </summary>
public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    #region State Lifecycle
    public override void EnterState()
    {
        // Al entrar, decidimos si el jugador debe estar en Idle o Run
        InitializeSubState();

        // Aplicamos fuerza inicial de pegado al suelo
        ApplyGroundingForce();
    }

    public override void UpdateState()
    {
        // Mantenemos la fuerza vertical negativa a menos que estemos intentando saltar
        if (!Context.JumpToConsume && !Context.HasBufferedJump)
        {
            ApplyGroundingForce();
        }

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Limpieza opcional al dejar el suelo
    }
    #endregion

    #region Logic
    /// <summary>
    /// Evalúa las prioridades de transición: 1. Salto, 2. Caída, 3. Dash.
    /// </summary>
    public override void CheckSwitchStates()
    {
        // 1. PRIORIDAD: SALTO
        if (Context.JumpToConsume || Context.HasBufferedJump)
        {
            if (HandleJumpAttempt()) return;
        }

        // 2. PRIORIDAD: CAÍDA (Si el sistema de colisiones deja de detectar suelo)
        if (!Context.Grounded)
        {
            SwitchState(Factory.Fall());
            return;
        }

        // 3. PRIORIDAD: DASH
        if (Context.DashToConsume && Context.CanDash)
        {
            HandleDashAttempt();
        }
    }

    /// <summary>
    /// Determina si el jugador debe comenzar en Idle o Run basado en el input horizontal.
    /// </summary>
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
    #endregion

    #region Helpers (Private)
    private void ApplyGroundingForce()
    {
        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, Context.Stats.GroundingForce);
    }

    private bool HandleJumpAttempt()
    {
        if (Context.StatsController.HasEnoughStamina(Context.Stats.JumpStaminaCost))
        {
            SwitchState(Factory.Jump());
            return true;
        }

        // Fallo por falta de estamina: limpiamos flags para evitar saltos accidentales en cola
        Context.JumpToConsume = false;
        Context.UseJumpBuffer();
        return false;
    }

    private void HandleDashAttempt()
    {
        if (Context.StatsController.HasEnoughStamina(Context.Stats.DashStaminaCost))
        {
            SwitchState(Factory.Dash());
        }
        else
        {
            Context.DashToConsume = false;
        }
    }
    #endregion
}