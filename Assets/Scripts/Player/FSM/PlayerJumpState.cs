using UnityEngine;

/// <summary>
/// Estado raíz que gestiona el impulso ascendente del jugador y la gravedad inicial.
/// Implementa el consumo de estamina, el uso de buffers/coyote y el corte de salto variable.
/// </summary>
public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    #region State Lifecycle
    public override void EnterState()
    {
        // 1. Limpieza de flags e intención de salto
        Context.JumpToConsume = false;
        Context.UseJumpBuffer();
        Context.UseCoyote();

        // 2. Ejecución física y eventos
        ExecuteJumpImpulse();

        // 3. Permitir movimiento horizontal durante el salto
        InitializeSubState();
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Opcional: Regenerar un poco de estamina al salir si se desea
        // Context.StatsController.RegenStamina();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Evalúa si el jugador debe pasar a estado de caída o si tocó el suelo inesperadamente.
    /// </summary>
    public override void CheckSwitchStates()
    {
        // Si la velocidad vertical es 0 o negativa, empezamos a caer
        if (Context.FrameVelocity.y <= 0)
        {
            SwitchState(Factory.Fall());
        }
        // Seguridad: Si tocamos suelo mientras subimos (ej. una plataforma móvil)
        else if (Context.Grounded && Context.FrameVelocity.y <= -0.1f)
        {
            SwitchState(Factory.Grounded());
        }
    }

    public override void InitializeSubState()
    {
        // Permite que el jugador use Idle o Run mientras está en el aire
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
    /// Aplica el impulso inicial de salto y notifica a los sistemas externos.
    /// </summary>
    private void ExecuteJumpImpulse()
    {
        // Aplicamos la fuerza vertical directa
        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, Context.Stats.JumpPower);

        // Gestión de recursos y eventos
        Context.StatsController.ConsumeStamina(Context.Stats.JumpStaminaCost);
        Context.InvokeJumpEvent();
    }

    /// <summary>
    /// Aplica gravedad personalizada. Incluye el multiplicador de "corte de salto" 
    /// para permitir saltos más cortos si se suelta el botón.
    /// </summary>
    private void HandleGravity()
    {
        float gravity = Context.Stats.FallAcceleration;

        // "Variable Jump Height": Si el jugador suelta el botón mientras sube, 
        // aumentamos la gravedad para frenar el ascenso rápidamente.
        if (!Context.FrameInputRef.JumpHeld && Context.FrameVelocity.y > 0)
        {
            gravity *= Context.Stats.JumpEndEarlyGravityModifier;
        }

        float newY = Mathf.MoveTowards(
            Context.FrameVelocity.y,
            -Context.Stats.MaxFallSpeed,
            gravity * Time.fixedDeltaTime
        );

        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, newY);
    }
    #endregion
}