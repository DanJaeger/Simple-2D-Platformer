using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
       : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }

    public override void CheckSwitchStates()
    {
        // 1. Si tocamos el suelo, volvemos a Grounded
        if (Context.Grounded)
        {
            SwitchState(Factory.Grounded());
        }
        // 2. Permitir Dash en el aire (si tus stats lo permiten)
        else if (Context.DashToConsume && Context.CanDash && Context.Stats.CanDashInAir)
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
        // 3. Coyote Time: Si presionas salto justo al empezar a caer
        else if (Context.JumpToConsume && Context.CanUseCoyote)
        {
            if (Context.StatsController.HasEnoughStamina(Context.Stats.JumpStaminaCost))
            {
                SwitchState(Factory.Jump());
            }
            else
            {
                Context.JumpToConsume = false;
                Context.UseJumpBuffer();
            }
        }
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {
        // Permitimos control horizontal durante la caída
        if (Mathf.Abs(Context.FrameInput.Move.x) < 0.01f)
        {
            SetSubState(Factory.Idle());
        }
        else
        {
            SetSubState(Factory.Run());
        }
    }

    private void HandleGravity()
    {
        // Aplicamos la aceleración de caída (gravedad) definida en tus stats
        float speedWithGravity = Mathf.MoveTowards(
            Context.FrameVelocity.y,
            -Context.Stats.MaxFallSpeed,
            Context.Stats.FallAcceleration * Time.fixedDeltaTime
        );

        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, speedWithGravity);
    }
}