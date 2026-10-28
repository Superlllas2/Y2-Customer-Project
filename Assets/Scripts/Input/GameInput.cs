using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Central Input System hub for gameplay. Create actions in code so the project
/// does not depend on a generated .inputactions C# class.
/// </summary>
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public InputAction Move { get; private set; }
    public InputAction Look { get; private set; }
    public InputAction Jump { get; private set; }
    public InputAction Sprint { get; private set; }
    public InputAction Interact { get; private set; }
    public InputAction RotateModifier { get; private set; }
    public InputAction ToggleDetector { get; private set; }
    public InputAction SpawnPump { get; private set; }
    public InputAction ToggleControls { get; private set; }
    public InputAction Point { get; private set; }
    public InputAction Click { get; private set; }

    /// <summary>When true, look/movement gameplay should pause (controls UI open).</summary>
    public bool IsControlsOpen { get; private set; }

    public event Action ControlsToggled;

    private InputActionMap _playerMap;
    private InputActionMap _uiMap;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        var go = new GameObject("GameSystems");
        go.AddComponent<GameInput>();
        go.AddComponent<UnityEngine.UIElements.UIDocument>();
        go.AddComponent<ControlsPanelController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildActions();
        EnableGameplay();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }

        _playerMap?.Disable();
        _uiMap?.Disable();
        _playerMap?.Dispose();
        _uiMap?.Dispose();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Main menu needs a free cursor; gameplay scenes lock via PlayerCam / SetControlsOpen(false).
        if (scene.name.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            IsControlsOpen = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void BuildActions()
    {
        _playerMap = new InputActionMap("Player");

        Move = _playerMap.AddAction("Move", InputActionType.Value);
        Move.expectedControlType = "Vector2";
        Move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        Look = _playerMap.AddAction("Look", InputActionType.Value);
        Look.expectedControlType = "Vector2";
        Look.AddBinding("<Mouse>/delta");

        Jump = _playerMap.AddAction("Jump", InputActionType.Button);
        Jump.AddBinding("<Keyboard>/space");

        Sprint = _playerMap.AddAction("Sprint", InputActionType.Button);
        Sprint.AddBinding("<Keyboard>/leftShift");

        Interact = _playerMap.AddAction("Interact", InputActionType.Button);
        Interact.AddBinding("<Mouse>/leftButton");

        RotateModifier = _playerMap.AddAction("RotateModifier", InputActionType.Button);
        RotateModifier.AddBinding("<Keyboard>/r");

        ToggleDetector = _playerMap.AddAction("ToggleDetector", InputActionType.Button);
        ToggleDetector.AddBinding("<Keyboard>/e");

        SpawnPump = _playerMap.AddAction("SpawnPump", InputActionType.Button);
        SpawnPump.AddBinding("<Keyboard>/8");
        SpawnPump.AddBinding("<Keyboard>/numpad8");

        _uiMap = new InputActionMap("UI");

        ToggleControls = _uiMap.AddAction("ToggleControls", InputActionType.Button);
        ToggleControls.AddBinding("<Keyboard>/h");
        ToggleControls.AddBinding("<Keyboard>/tab");

        Point = _uiMap.AddAction("Point", InputActionType.Value);
        Point.expectedControlType = "Vector2";
        Point.AddBinding("<Mouse>/position");

        Click = _uiMap.AddAction("Click", InputActionType.Button);
        Click.AddBinding("<Mouse>/leftButton");

        ToggleControls.performed += _ =>
        {
            SetControlsOpen(!IsControlsOpen);
            ControlsToggled?.Invoke();
        };
    }

    public void EnableGameplay()
    {
        _playerMap.Enable();
        _uiMap.Enable();
    }

    public void SetControlsOpen(bool open)
    {
        IsControlsOpen = open;

        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Keep UI map; suppress look by consumers checking IsControlsOpen
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public Vector2 MoveValue => Move != null ? Move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 LookDelta => Look != null ? Look.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 PointerPosition => Point != null ? Point.ReadValue<Vector2>() : Vector2.zero;

    public bool SprintHeld => Sprint != null && Sprint.IsPressed();
    public bool JumpPressed => Jump != null && Jump.WasPressedThisFrame();
    public bool InteractPressed => Interact != null && Interact.WasPressedThisFrame();
    public bool InteractReleased => Interact != null && Interact.WasReleasedThisFrame();
    public bool InteractHeld => Interact != null && Interact.IsPressed();
    public bool RotatePressed => RotateModifier != null && RotateModifier.WasPressedThisFrame();
    public bool RotateReleased => RotateModifier != null && RotateModifier.WasReleasedThisFrame();
    public bool RotateHeld => RotateModifier != null && RotateModifier.IsPressed();
    public bool DetectorPressed => ToggleDetector != null && ToggleDetector.WasPressedThisFrame();
    public bool SpawnPumpPressed => SpawnPump != null && SpawnPump.WasPressedThisFrame();
    public bool ClickPressed => Click != null && Click.WasPressedThisFrame();

    public string BindingDisplay(InputAction action, int bindingIndex = 0)
    {
        if (action == null || action.bindings.Count == 0)
        {
            return "?";
        }

        var index = Mathf.Clamp(bindingIndex, 0, action.bindings.Count - 1);
        return action.GetBindingDisplayString(index);
    }
}
