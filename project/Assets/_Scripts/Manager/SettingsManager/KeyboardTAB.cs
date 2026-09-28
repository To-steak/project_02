using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class KeyboardTAB : SettingsTAB
{
    [SerializeField] private Slider _mouseSensitivitySlider;
    [SerializeField] private TMP_Text _mouseSensitivityLabel;

    private const int MIN_MOUSE_SENSITIVITY = 1;
    private const int MAX_MOUSE_SENSITIVITY = 100;
    private const float SENSITIVITY_SCALE = 100f;

    protected override void OnBind()
    {
        SetupSlider(_mouseSensitivitySlider, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);

        _mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    public override void Refresh()
    {
        float display = Mathf.Round(CurrentSettings.MouseSensitivity * SENSITIVITY_SCALE);
        display = Mathf.Clamp(display, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        _mouseSensitivitySlider.SetValueWithoutNotify(display);
        UpdateLabel(_mouseSensitivityLabel, display);
    }

    private void OnMouseSensitivityChanged(float value)
    {
        CurrentSettings.MouseSensitivity = value / SENSITIVITY_SCALE;
        UpdateLabel(_mouseSensitivityLabel, value);
        NotifyChanged();
    }
}