using UnityEngine;

/// <summary>
/// Estado raíz que gestiona la habilidad de Dash. 
/// Desplaza al jugador a alta velocidad en una dirección fija, ignorando la gravedad
/// y bloqueando otras transiciones hasta que finalice la duración establecida.
/// </summary>
public class PlayerDashState : PlayerBaseState
{
    private float _dashStartTime;
    private float _dashDirection;

    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    #region State Lifecycle
    public override void EnterState()
    {
        // 1. Limpieza de consumo para evitar ejecuciones en bucle
        Context.DashToConsume = false;

        // 2. Configuración de estados y cronómetro
        _dashStartTime = Time.time;
        Context.IsDashing = true;
        Context.CanDash = false;

        // 3. Ejecución de recursos y eventos
        Context.StatsController.ConsumeStamina(Context.Stats.DashStaminaCost);
        Context.InvokeDashEvent();

        // 4. Lógica de dirección e impulso inicial
        SetDashDirection();
        ApplyDashVelocity();

        // El Dash suele ser un estado atómico (sin sub-estados de movimiento)
        InitializeSubState();
    }

    public override void UpdateState()
    {
        // Mantenemos la velocidad constante y el eje Y en 0 (sin gravedad)
        ApplyDashVelocity();

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Context.IsDashing = false;

        // Iniciamos el enfriamiento de la habilidad al terminar la acción
        Context.StartDashCooldown();

        // Opcional: Permitir una regeneración inmediata tras el esfuerzo
        Context.StatsController.RegenStamina();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Finaliza el estado una vez transcurrido el tiempo de duración.
    /// </summary>
    public override void CheckSwitchStates()
    {
        if (Time.time >= _dashStartTime + Context.Stats.DashDuration)
        {
            TransitionAfterDash();
        }
    }

    public override void InitializeSubState()
    {
        // Normalmente no hay sub-estados durante el dash para evitar que
        // la lógica de Run/Idle interfiera con la velocidad del dash.
    }
    #endregion

    #region Helpers (Private)
    private void SetDashDirection()
    {
        // Prioridad 1: Input horizontal actual.
        // Prioridad 2: Dirección visual (donde mira el personaje).
        if (Context.FrameInputRef.Move.x != 0)
        {
            _dashDirection = Mathf.Sign(Context.FrameInputRef.Move.x);
        }
        else
        {
            _dashDirection = Mathf.Sign(Context.transform.localScale.x);
        }
    }

    private void ApplyDashVelocity()
    {
        // Forzamos la velocidad horizontal y anulamos la vertical (gravedad 0)
        Context.FrameVelocity = new Vector2(_dashDirection * Context.Stats.DashPower, 0);
    }

    private void TransitionAfterDash()
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
    #endregion
}