using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class GameSettingsSection : MonoBehaviour
{
    private GameSettingsPresenter _presenter;
    protected GameSettings CurrentSettings => _presenter.Current;

    public void Bind(GameSettingsPresenter presenter)
    {
        _presenter = presenter;
        OnBind();
    }

    public abstract void Refresh();
    protected abstract void OnBind();

    protected void NotifyChanged()
    {
        _presenter.NotifyChanged();
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