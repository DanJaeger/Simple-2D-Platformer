using UnityEngine;
public class PlayerJumpState : PlayerBaseState, IRootState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }
    public override void CheckSwitchStates()
    {
        if (Context.Grounded && Context.FrameVelocity.y <= -0.1f)
        {
            SwitchState(Factory.Grounded());
        }
        else if (Context.FrameVelocity.y <= 0)
        {
            SwitchState(Factory.Fall());
        }
    }

    public override void EnterState()
    {
        Context.JumpToConsume = false;
        Context.UseJumpBuffer();
        Context.UseCoyote();

        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, Context.Stats.JumpPower);

        ExecuteJump();
        InitializeSubState();
    }

    public override void ExitState()
    {
        Context.StatsController.RegenStamina();
    }

    public override void InitializeSubState()
    {
        if (Mathf.Abs(Context.FrameInput.Move.x) < 0.01f)
        {
            SetSubState(Factory.Idle());
        }
        else
        {
            SetSubState(Factory.Run());
        }
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }
    private void ExecuteJump()
    {
        // Aplicamos la fuerza de salto usando tus Stats
        float jumpVelocity = Context.Stats.JumpPower;
        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, jumpVelocity);

        // Consumimos estamina y disparamos el evento (Patrón Observer)
        Context.StatsController.ConsumeStamina(Context.Stats.JumpStaminaCost);
        Context.InvokeJumpEvent();
    }
    private void HandleGravity()
    {
        var inAirGravity = Context.Stats.FallAcceleration;

        if (!Context.FrameInput.JumpHeld && Context.FrameVelocity.y > 0)
        {
            inAirGravity *= Context.Stats.JumpEndEarlyGravityModifier;
        }

        // USA SIEMPRE fixedDeltaTime dentro de FixedUpdate para cálculos de velocidad
        float newY = Mathf.MoveTowards(
            Context.FrameVelocity.y,
            -Context.Stats.MaxFallSpeed,
            inAirGravity * Time.fixedDeltaTime
        );

        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, newY);
    }
}