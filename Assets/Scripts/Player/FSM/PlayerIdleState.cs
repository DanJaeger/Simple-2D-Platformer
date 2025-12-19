
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }
    public override void CheckSwitchStates()
    {
        if (Mathf.Abs(Context.FrameInput.Move.x) > 0.01f)
        {
            SwitchState(Factory.Run());
        }
    }

    public override void EnterState()
    {
        
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        ApplyDeceleration();
    }
    private void ApplyDeceleration()
    {
        // Determinamos qué desaceleración usar dependiendo de si estamos en el suelo o aire
        float deceleration = Context.Grounded ? Context.Stats.GroundDeceleration : Context.Stats.AirDeceleration;

        // Llevamos la velocidad X hacia 0
        float newX = Mathf.MoveTowards(Context.FrameVelocity.x, 0, deceleration * Time.fixedDeltaTime);

        Context.FrameVelocity = new Vector2(newX, Context.FrameVelocity.y);
    }
}