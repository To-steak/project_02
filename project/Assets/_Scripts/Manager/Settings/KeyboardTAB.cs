using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class KeyboardTAB : SettingsTAB
{
    [SerializeField] Slider mouseSensitivitySlider;
    [SerializeField] TMP_Text mouseSensitivityLabel;

    const float MIN_MOUSE_SENSITIVITY = 0.01f;
    const float MAX_MOUSE_SENSITIVITY = 1.0f;

    protected override void OnBind()
    {
        mouseSensitivitySlider.minValue = MIN_MOUSE_SENSITIVITY;
        mouseSensitivitySlider.maxValue = MAX_MOUSE_SENSITIVITY;
        mouseSensitivitySlider.wholeNumbers = false;

        mouseSensitivitySlider.onValueChanged.AddListener(value =>
        {
            Current.MouseSensitivity = value;
            UpdateLabel(value);
            NotifyChanged();
        });
    }

    public override void Refresh()
    {
        float value = Mathf.Clamp(Current.MouseSensitivity, MIN_MOUSE_SENSITIVITY, MAX_MOUSE_SENSITIVITY);
        mouseSensitivitySlider.SetValueWithoutNotify(value);
        UpdateLabel(value);
    }

    private void UpdateLabel(float value)
    {
        if (mouseSensitivityLabel != null)
        {
            mouseSensitivityLabel.text = value.ToString("0.0");
        }
    }
}