using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }
    public override void CheckSwitchStates()
    {
        if (Mathf.Abs(Context.FrameInput.Move.x) < 0.01f)
        {
            SwitchState(Factory.Idle());
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
        Move();
        CheckSwitchStates();
    }
    private void Move()
    {
        // Extraemos la lógica de tu HandleDirection que tenías en el StateMachine
        // Pero ahora solo la parte que aplica cuando HAY movimiento
        float targetSpeed = Context.FrameInput.Move.x * Context.Stats.MaxSpeed;

        // Accedemos a la variable interna del Contexto para modificar la velocidad
        // Nota: Asegúrate de que _frameVelocity sea accesible (puedes hacerle un getter/setter o ponerla interna)
        float newX = Mathf.MoveTowards(Context.FrameVelocity.x, targetSpeed, Context.Stats.Acceleration * Time.fixedDeltaTime);

        Context.FrameVelocity = new Vector2(newX, Context.FrameVelocity.y);
    }
}