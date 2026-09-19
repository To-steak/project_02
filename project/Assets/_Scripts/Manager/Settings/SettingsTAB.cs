using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameManager;

public abstract class SettingsTAB : MonoBehaviour
{
    private SettingsManager _manager;
    protected GameSettings CurrentSettings => _manager.Current;

    public void Bind(SettingsManager manager)
    {
        _manager = manager;
        OnBind();
    }

    public abstract void Refresh();
    protected abstract void OnBind();

    protected void NotifyChanged()
    {
        _manager.OnSettingsChanged();
    }

    protected static void SetupSlider(Slider slider, int min, int max)
    {
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
    }

    protected static void UpdateLabel(TMP_Text label, float value)
    {
        if (label != null)
        {
            label.text = Mathf.RoundToInt(value).ToString();
        }
    }
}