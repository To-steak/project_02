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
        mouseSensitivitySlider.minValue = MIN_MOUSE_SENSITIVITY;
        mouseSensitivitySlider.maxValue = MAX_MOUSE_SENSITIVITY;
        mouseSensitivitySlider.wholeNumbers = true;

        mouseSensitivitySlider.onValueChanged.AddListener(value =>
        {
            Current.MouseSensitivity = value / SENSITIVITY_SCALE;
            UpdateLabel(value);
            NotifyChanged();
        });
    }

    public override void Refresh()
    {
        float display = Mathf.Round(Current.MouseSensitivity * SENSITIVITY_SCALE);
        display = Mathf.Clamp(display, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        mouseSensitivitySlider.SetValueWithoutNotify(display);
        UpdateLabel(display);
    }

    private void UpdateLabel(float value)
    {
        if (mouseSensitivityLabel != null)
        {
            mouseSensitivityLabel.text = Mathf.RoundToInt(value).ToString();
        }
    }
}