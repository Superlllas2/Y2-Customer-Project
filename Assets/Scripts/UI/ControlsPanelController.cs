using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ControlsPanelController : MonoBehaviour
{
    public static ControlsPanelController Instance { get; private set; }

    private UIDocument _document;
    private VisualElement _panel;
    private VisualElement _hint;
    private VisualElement _rotatePrompt;
    private bool _bound;
    private bool _rotatePromptVisible;

    private void Awake()
    {
        Instance = this;
        _document = GetComponent<UIDocument>();
        EnsurePanelSettings();

        var tree = Resources.Load<VisualTreeAsset>("UI/ControlsPanel");
        if (tree != null)
        {
            _document.visualTreeAsset = tree;
        }
        else
        {
            Debug.LogWarning("Controls UXML missing at Resources/UI/ControlsPanel");
        }
    }

    private void EnsurePanelSettings()
    {
        var theme = Resources.Load<ThemeStyleSheet>("UI/UnityDefaultRuntimeTheme");
        var settings = Resources.Load<PanelSettings>("UI/GamePanelSettings");
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.sortingOrder = 100;
        }

        if (theme != null)
        {
            settings.themeStyleSheet = theme;
        }
        else
        {
            Debug.LogWarning("UI/UnityDefaultRuntimeTheme missing — UITK may not render.");
        }

        _document.panelSettings = settings;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.ControlsToggled += Refresh;
        }
    }

    private void OnDisable()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.ControlsToggled -= Refresh;
        }
    }

    private void Start()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.ControlsToggled -= Refresh;
            GameInput.Instance.ControlsToggled += Refresh;
        }

        Refresh();
        ApplyRotatePromptVisibility();
    }

    private void LateUpdate()
    {
        // UIDocument root can be null for a frame after assigning visualTreeAsset.
        if (!_bound)
        {
            Refresh();
        }
    }

    public void SetRotatePromptVisible(bool visible)
    {
        _rotatePromptVisible = visible;
        ApplyRotatePromptVisibility();
    }

    private void BindUI()
    {
        if (_document == null)
        {
            return;
        }

        if (_document.rootVisualElement == null && _document.visualTreeAsset != null)
        {
            // Force build if Awake assignment did not create a root yet.
            _document.visualTreeAsset = _document.visualTreeAsset;
        }

        if (_bound || _document.rootVisualElement == null)
        {
            return;
        }

        var root = _document.rootVisualElement;
        var style = Resources.Load<StyleSheet>("UI/ControlsPanel");
        if (style != null && !root.styleSheets.Contains(style))
        {
            root.styleSheets.Add(style);
        }

        _panel = root.Q<VisualElement>("panel");
        _hint = root.Q<Label>("hint");
        _rotatePrompt = root.Q<VisualElement>("rotate-prompt");
        if (_rotatePrompt != null)
        {
            _rotatePrompt.pickingMode = PickingMode.Ignore;
            foreach (var child in _rotatePrompt.Children())
            {
                child.pickingMode = PickingMode.Ignore;
            }
        }

        var close = root.Q<Button>("close-button");
        if (close != null)
        {
            close.clicked += () =>
            {
                if (GameInput.Instance == null)
                {
                    return;
                }

                GameInput.Instance.SetControlsOpen(false);
                Refresh();
            };
        }

        ApplyBindingLabels(root);
        _bound = true;
    }

    private void ApplyBindingLabels(VisualElement root)
    {
        var input = GameInput.Instance;
        if (input == null)
        {
            return;
        }

        SetLabel(root, "bind-sprint", input.BindingDisplay(input.Sprint));
        SetLabel(root, "bind-jump", input.BindingDisplay(input.Jump));
        SetLabel(root, "bind-detector", input.BindingDisplay(input.ToggleDetector));
        SetLabel(root, "bind-interact", input.BindingDisplay(input.Interact));
        SetLabel(root, "bind-rotate-snapped", input.BindingDisplay(input.RotateModifier));
        SetLabel(root, "bind-pump", input.BindingDisplay(input.SpawnPump));
        SetLabel(root, "rotate-prompt-key", input.BindingDisplay(input.RotateModifier));
    }

    private static void SetLabel(VisualElement root, string name, string text)
    {
        var label = root.Q<Label>(name);
        if (label != null)
        {
            label.text = text;
        }
    }

    private void Refresh()
    {
        BindUI();
        if (_panel == null)
        {
            return;
        }

        var open = GameInput.Instance != null && GameInput.Instance.IsControlsOpen;
        _panel.EnableInClassList("panel--open", open);
        if (_hint != null)
        {
            _hint.EnableInClassList("hint--hidden", open);
        }

        ApplyRotatePromptVisibility();
    }

    private void ApplyRotatePromptVisibility()
    {
        BindUI();
        if (_rotatePrompt == null)
        {
            return;
        }

        var controlsOpen = GameInput.Instance != null && GameInput.Instance.IsControlsOpen;
        _rotatePrompt.EnableInClassList("rotate-prompt--hidden", !_rotatePromptVisible || controlsOpen);
    }
}
