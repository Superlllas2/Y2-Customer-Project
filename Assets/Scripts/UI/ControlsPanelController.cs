using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ControlsPanelController : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _panel;
    private VisualElement _hint;
    private bool _bound;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        if (_document.panelSettings == null)
        {
            _document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _document.panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _document.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
        }

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
        // GameInput Awake may run after us on first creation — retry bind once.
        if (GameInput.Instance != null)
        {
            GameInput.Instance.ControlsToggled -= Refresh;
            GameInput.Instance.ControlsToggled += Refresh;
        }

        Refresh();
    }

    private void BindUI()
    {
        if (_bound || _document == null || _document.rootVisualElement == null)
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
        SetLabel(root, "bind-rotate", "Hold " + input.BindingDisplay(input.RotateModifier) + " + Mouse");
        SetLabel(root, "bind-pump", input.BindingDisplay(input.SpawnPump));
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
    }
}
