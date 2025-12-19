using System.Collections.Generic;

/// <summary>
/// Listado de estados disponibles para el jugador.
/// Facilita el acceso mediante claves en lugar de strings o comparaciones de clase.
/// </summary>
public enum PlayerStates
{
    Idle,
    Run,
    Grounded,
    Jump,
    Dash,
    Fall
}

/// <summary>
/// Fábrica encargada de instanciar y almacenar todos los estados del jugador.
/// Utiliza el patrón Flyweight para reutilizar instancias y optimizar memoria.
/// </summary>
public class PlayerStateFactory
{
    private readonly PlayerStateMachine _context;
    private readonly Dictionary<PlayerStates, PlayerBaseState> _states = new();

    /// <summary>
    /// Inicializa la fábrica y pre-genera todos los estados posibles.
    /// </summary>
    /// <param name="currentContext">Referencia a la máquina de estados principal.</param>
    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;

        // Registro de instancias
        _states[PlayerStates.Idle] = new PlayerIdleState(_context, this);
        _states[PlayerStates.Run] = new PlayerRunState(_context, this);
        _states[PlayerStates.Jump] = new PlayerJumpState(_context, this);
        _states[PlayerStates.Dash] = new PlayerDashState(_context, this);
        _states[PlayerStates.Grounded] = new PlayerGroundedState(_context, this);
        _states[PlayerStates.Fall] = new PlayerFallState(_context, this);
    }

    #region Accessors (Getters)
    // Estos métodos permiten a los estados transicionar usando Factory.Idle(), etc.

    public PlayerBaseState Idle() => _states[PlayerStates.Idle];
    public PlayerBaseState Run() => _states[PlayerStates.Run];
    public PlayerBaseState Jump() => _states[PlayerStates.Jump];
    public PlayerBaseState Dash() => _states[PlayerStates.Dash];
    public PlayerBaseState Grounded() => _states[PlayerStates.Grounded];
    public PlayerBaseState Fall() => _states[PlayerStates.Fall];
    #endregion
}