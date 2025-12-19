using UnityEngine;

/// <summary>
/// Clase base abstracta para el sistema de Máquina de Estados Jerárquica (HFSM).
/// Gestiona la lógica de entrada, salida y la relación entre Super-Estados y Sub-Estados.
/// </summary>
public abstract class PlayerBaseState
{
    #region Fields
    private bool _isRootState = false;
    private PlayerStateMachine _context;
    private PlayerStateFactory _factory;
    private PlayerBaseState _currentSubState;
    private PlayerBaseState _currentSuperState;
    #endregion

    #region Properties
    protected bool IsRootState { get => _isRootState; set => _isRootState = value; }
    protected PlayerStateMachine Context => _context;
    protected PlayerStateFactory Factory => _factory;
    #endregion

    /// <summary>
    /// Constructor base para inicializar las referencias del contexto y la factory.
    /// </summary>
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _context = currentContext;
        _factory = playerStateFactory;
    }

    #region Abstract Methods
    /// <summary> Se ejecuta una vez al entrar al estado. </summary>
    public abstract void EnterState();

    /// <summary> Se ejecuta cada frame (llamado desde FixedUpdate en este proyecto). </summary>
    public abstract void UpdateState();

    /// <summary> Se ejecuta una vez antes de cambiar a un nuevo estado. </summary>
    public abstract void ExitState();

    /// <summary> Contiene las condiciones necesarias para transicionar a otros estados. </summary>
    public abstract void CheckSwitchStates();

    /// <summary> Define qué sub-estado debe iniciarse al entrar en este estado. </summary>
    public abstract void InitializeSubState();
    #endregion

    #region State Logic
    /// <summary>
    /// Actualiza el estado actual y recursivamente su sub-estado activo.
    /// </summary>
    public void UpdateStates()
    {
        UpdateState();
        _currentSubState?.UpdateStates();
    }

    /// <summary>
    /// Gestiona la transición hacia un nuevo estado, notificando al contexto o al super-estado.
    /// </summary>
    /// <param name="newState">El estado al que se desea transicionar.</param>
    protected void SwitchState(PlayerBaseState newState)
    {
        // 1. Salir del estado actual
        ExitState();

        // 2. Entrar en el nuevo estado
        newState.EnterState();

        // 3. Reasignar referencias según la jerarquía
        if (_isRootState)
        {
            _context.CurrentState = newState;
        }
        else if (_currentSuperState != null)
        {
            // Si este es un sub-estado, le pedimos al padre que actualice su referencia
            _currentSuperState.SetSubState(newState);
        }
    }

    /// <summary>
    /// Establece el padre (super-estado) de este estado.
    /// </summary>
    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    /// <summary>
    /// Establece el hijo (sub-estado) de este estado y se asigna como su padre automáticamente.
    /// </summary>
    protected void SetSubState(PlayerBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
    #endregion
}