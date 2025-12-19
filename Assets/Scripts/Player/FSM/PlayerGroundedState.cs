
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState, IRootState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }
    public override void CheckSwitchStates()
    {
        if (Context.JumpToConsume || Context.HasBufferedJump)
        {
            if (Context.StatsController.HasEnoughStamina(Context.Stats.JumpStaminaCost))
            {
                SwitchState(Factory.Jump());
                return;
            }
            else
            {
                Context.JumpToConsume = false;
                Context.UseJumpBuffer();
            }
        }
        // 2. Si el sistema de colisiones detecta que ya no hay suelo
        else if (!Context.Grounded)
        {

            SwitchState(Factory.Fall());
            return;
        }
        // 3. Si se presiona Dash y podemos dashear
        else if (Context.DashToConsume && Context.CanDash)
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
    }

    public override void EnterState()
    {
        InitializeSubState();

        Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, Context.Stats.GroundingForce);
    }

    public override void ExitState()
    {

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
        if (!Context.JumpToConsume && !Context.HasBufferedJump)
        {
            Context.FrameVelocity = new Vector2(Context.FrameVelocity.x, Context.Stats.GroundingForce);
        }

        CheckSwitchStates();
    }
}