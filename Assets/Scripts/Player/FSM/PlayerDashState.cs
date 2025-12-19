using UnityEngine;

public class PlayerDashState : PlayerBaseState, IRootState
{
    private float _dashStartTime;
    private float _dashDirection;

    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        // 1. LIMPIEZA INMEDIATA DEL INPUT (Vital para evitar el bucle)
        Context.DashToConsume = false;

        // 2. Configuración de tiempo y flags
        _dashStartTime = Time.time;
        Context.IsDashing = true;
        Context.CanDash = false;

        // 3. Consumo de recursos
        Context.StatsController.ConsumeStamina(Context.Stats.DashStaminaCost);
        Context.InvokeDashEvent();

        // 4. Determinar dirección del dash
        // Priorizamos el input horizontal; si no hay, usamos la dirección visual (localScale)
        _dashDirection = Context.FrameInput.Move.x != 0
            ? Mathf.Sign(Context.FrameInput.Move.x)
            : Mathf.Sign(Context.transform.localScale.x);

        // 5. Aplicar velocidad inicial del dash (congelamos el eje Y)
        Context.FrameVelocity = new Vector2(_dashDirection * Context.Stats.DashPower, 0);

        InitializeSubState();
    }

    public override void UpdateState()
    {
        // Mantenemos la velocidad constante durante el dash ignorando la gravedad
        Context.FrameVelocity = new Vector2(_dashDirection * Context.Stats.DashPower, 0);

        CheckSwitchStates();
    }

    public override void CheckSwitchStates()
    {
        // Si el tiempo del dash ha terminado
        if (Time.time >= _dashStartTime + Context.Stats.DashDuration)
        {
            if (Context.Grounded)
            {
                SwitchState(Factory.Grounded());
            }
            else
            {
                SwitchState(Factory.Fall());
            }
        }
    }

    public override void ExitState()
    {
        Context.IsDashing = false;
        // Iniciamos el cooldown al salir
        Context.StartDashCooldown();
        Context.StatsController.RegenStamina();
    }

    public override void InitializeSubState() { }
}