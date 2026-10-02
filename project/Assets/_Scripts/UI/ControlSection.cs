using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSection : SettingsSection
{
    [SerializeField] private Slider _mouseSensitivitySlider;
    [SerializeField] private TMP_Text _mouseSensitivityLabel;

    private const int MIN_MOUSE_SENSITIVITY = 1;
    private const int MAX_MOUSE_SENSITIVITY = 100;
    private const float SENSITIVITY_SCALE = 100f;

    [SerializeField] private RebindEntry[] _bindings;
    private InputActionRebindingExtensions.RebindingOperation _rebind;
    private static InputActionAsset Asset => InputService.Actions.asset;

    private void OnDisable()
    {
        _rebind?.Cancel();
    }

    protected override void OnBind()
    {
        SetupSlider(_mouseSensitivitySlider, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        _mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        foreach (var item in _bindings)
        {
            var action = Asset.FindAction(item.Action?.Trim());
            if (action == null)
            {
                Debug.LogError($"[{nameof(ControlSection)}] no action: '{item.Action}'", this);
                continue;
            }

            item.BindingIndex = FindBindingIndex(action, item.CompositePart?.Trim());
            if (item.BindingIndex < 0)
            {
                Debug.LogError($"[{nameof(ControlSection)}] no binding: {item.Action} / Part='{item.CompositePart}'", this);
                continue;
            }

            if (item.Button == null)
            {
                continue;
            }

            var captured = item;
            item.Button.onClick.AddListener(() => StartRebind(captured));
        }
    }

    public override void Refresh()
    {
        float display = Mathf.Round(CurrentSettings.MouseSensitivity * SENSITIVITY_SCALE);
        display = Mathf.Clamp(display, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        _mouseSensitivitySlider.SetValueWithoutNotify(display);

        UpdateLabel(_mouseSensitivityLabel, display);
        RefreshBindingLabel();
    }

    private void OnMouseSensitivityChanged(float value)
    {
        CurrentSettings.MouseSensitivity = value / SENSITIVITY_SCALE;
        UpdateLabel(_mouseSensitivityLabel, value);
        NotifyChanged();
    }

    private void StartRebind(RebindEntry key)
    {
        var action = Asset.FindAction(key.Action);
        bool enabled = action.enabled;
        action.Disable();

        key.Label.text = "...";
        SetBindButtonsInteractable(false);

        _rebind = action.PerformInteractiveRebinding(key.BindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(_ => CallbackRebind(action, key.BindingIndex, enabled, true))
            .OnCancel(_ => CallbackRebind(action, key.BindingIndex, enabled, false))
            .Start();
    }

    private void CallbackRebind(InputAction action, int bindingIndex, bool enabled, bool changed)
    {
        _rebind.Dispose();
        _rebind = null;

        if (changed)
        {
            var bind = action.bindings[bindingIndex];
            if (bind.overridePath == bind.path)
            {
                action.RemoveBindingOverride(bindingIndex);
            }
        }

        if (enabled)
        {
            action.Enable();
        }

        SetBindButtonsInteractable(true);

        if (changed)
        {
            CurrentSettings.KeyBinding = SaveOverride();
            NotifyChanged();
        }

        RefreshBindingLabel();
    }

    private void RefreshBindingLabel()
    {
        foreach (var item in _bindings)
        {
            if (item.BindingIndex < 0 || item.Label == null)
            {
                continue;
            }

            var action = Asset.FindAction(item.Action);
            item.Label.text = action.GetBindingDisplayString(item.BindingIndex);
        }
    }

    private void SetBindButtonsInteractable(bool value)
    {
        foreach (var item in _bindings)
        {
            if (item.Button != null)
            {
                item.Button.interactable = value;
            }
        }
    }

    private static int FindBindingIndex(InputAction action, string part)
    {
        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            var bind = bindings[i];
            if (string.IsNullOrEmpty(part))
            {
                if (!bind.isComposite && !bind.isPartOfComposite)
                {
                    return i;
                }
            }
            else if (bind.isPartOfComposite && string.Equals(bind.name, part, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string SaveOverride()
    {
        foreach (var bind in Asset.bindings)
        {
            if (!string.IsNullOrEmpty(bind.overridePath))
            {
                return Asset.SaveBindingOverridesAsJson();
            }
        }

        return string.Empty;
    }
}