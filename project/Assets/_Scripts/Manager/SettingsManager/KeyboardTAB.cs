using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class KeyboardTAB : SettingsTAB
{
    [SerializeField] Slider mouseSensitivitySlider;
    [SerializeField] TMP_Text mouseSensitivityLabel;

    const int MIN_MOUSE_SENSITIVITY = 1;
    const int MAX_MOUSE_SENSITIVITY = 100;
    const float SENSITIVITY_SCALE = 100f;

    protected override void OnBind()
    {
        SetupSlider(mouseSensitivitySlider, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    public override void Refresh()
    {
        float display = Mathf.Round(CurrentSettings.MouseSensitivity * SENSITIVITY_SCALE);
        display = Mathf.Clamp(display, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        mouseSensitivitySlider.SetValueWithoutNotify(display);
        UpdateLabel(mouseSensitivityLabel, display);
    }

    private void OnMouseSensitivityChanged(float value)
    {
        CurrentSettings.MouseSensitivity = value;
        UpdateLabel(mouseSensitivityLabel, value);
        NotifyChanged();
    }
}