using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Deadzone Values")]
    [SerializeField] private float leftStickDeadZoneValue = 0.3f;

    private readonly Dictionary<string, InputAction> actions = new();
    private readonly string[] actionNames =
    {
        "Move",
        "Look",
        "Attack",
        "Interact",
        "Pause",
        "Jump",
        "Crouch",
        "SwitchCameraView"
    };


    // --- Input references ---
    // Raw data from input detection
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool AttackDown { get; private set; }
    public bool JumpDown { get; private set; }
    public bool CrouchDown { get; private set; }

    // Logical data from input detection
    public bool AttackTriggered => actions["Attack"].triggered;
    public bool JumpTriggered => actions["Jump"].triggered;
    public bool CrouchTriggered => actions["Crouch"].triggered;
    public bool PauseTriggered => actions["Pause"].triggered;
    public bool InteractTriggered => actions["Interact"].triggered;
    public bool CamSwitchTriggered => actions["SwitchCameraView"].triggered;

    #region  Singleton implementation

    /// <summary>
    /// Access to the player's inputs
    /// </summary>
    public static PlayerInputHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (string name in actionNames)
            actions[name] = playerControls.FindActionMap("Player").FindAction(name);

        RegisterInputActions();

        InputSystem.settings.defaultDeadzoneMin = leftStickDeadZoneValue;
    }

    #endregion

    /// <summary>
    /// Add the various input detections into the action events.
    /// </summary>
    private void RegisterInputActions()
    {
        actions["Move"].performed += context => MoveInput = context.ReadValue<Vector2>();
        actions["Move"].canceled += context => MoveInput = Vector2.zero;

        actions["Look"].performed += context => LookInput = context.ReadValue<Vector2>();
        actions["Look"].canceled += context => LookInput = Vector2.zero;

        actions["Attack"].performed += context => AttackDown = true;
        actions["Attack"].canceled += context => AttackDown = false;

        actions["Jump"].performed += context => JumpDown = true;
        actions["Jump"].canceled += context => JumpDown = false;

        actions["Crouch"].performed += context => CrouchDown = true;
        actions["Crouch"].canceled += context => CrouchDown = false;
    }

    private void OnEnable()
    {
        foreach (var (key, action) in actions) action.Enable();
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        foreach (var (key, action) in actions) action.Disable();
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    #region Device Management

    /// <summary>
    /// Announce whenever a device is disconnected / connected
    /// </summary>
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected: Debug.Log("Device Disconnected: " + device.name); break;
            case InputDeviceChange.Reconnected: Debug.Log("Device Reconnected: " + device.name); break;
        }
    }

    /// <summary>
    /// Announce every active device.
    /// </summary>
    private void PrintDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device.enabled)
            {
                Debug.Log("Active Device: " + device.name);
            }
        }
    }

    #endregion
}
