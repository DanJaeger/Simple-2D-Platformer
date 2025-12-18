using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton Input Manager using the new Unity Input System.
/// Provides access to player movement, camera look, and click input.
/// </summary>
public class InputManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    // --- Singleton ---
    private static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("No InputManager instance found in the scene!");
            return _instance;
        }
    }

    // --- Input Values ---
    public Vector2 PlayerMovementInput { get; private set; }
    public bool Interact { get; private set; }
    public bool Jump { get; private set; }
    public bool Dash { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple InputManager instances detected. Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        _instance = this;

        SetupInput();
    }

    /// <summary>
    /// Initializes the PlayerInput and subscribes to input actions.
    /// </summary>
    private void SetupInput()
    {
        _playerInput = new PlayerInput();

        // Movement
        _playerInput.Locomotion.Move.started += OnPlayerMovementInput;
        _playerInput.Locomotion.Move.performed += OnPlayerMovementInput;
        _playerInput.Locomotion.Move.canceled += OnPlayerMovementInput;

        // Interact
        _playerInput.Locomotion.Interact.started += OnInteractInput;
        _playerInput.Locomotion.Interact.performed += OnInteractInput;
        _playerInput.Locomotion.Interact.canceled += OnInteractInput;

        // Jump
        _playerInput.Locomotion.Jump.started += OnJumpInput;
        _playerInput.Locomotion.Jump.performed += OnJumpInput;
        _playerInput.Locomotion.Jump.canceled += OnJumpInput;

        // Dash
        _playerInput.Locomotion.Dash.started += OnDashInput;
        _playerInput.Locomotion.Dash.performed += OnDashInput;
        _playerInput.Locomotion.Dash.canceled += OnDashInput;
    }


    /// <summary>
    /// Handles movement input (WASD or analog stick).
    /// </summary>
    private void OnPlayerMovementInput(InputAction.CallbackContext context)
    {
        PlayerMovementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles Interact input (E).
    /// </summary>
    private void OnInteractInput(InputAction.CallbackContext context)
    {
        Interact = context.ReadValueAsButton();
    }

    /// <summary>
    /// Handles Jump input (Space).
    /// </summary>
    private void OnJumpInput(InputAction.CallbackContext context)
    {
        Jump = context.ReadValueAsButton();
    }

    /// <summary>
    /// Handles Dash input (Left Shift).
    /// </summary>
    private void OnDashInput(InputAction.CallbackContext context)
    {
        Dash = context.ReadValueAsButton();
    }
    private void OnEnable()
    {
        _playerInput.Locomotion.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Locomotion.Disable();
    }
}