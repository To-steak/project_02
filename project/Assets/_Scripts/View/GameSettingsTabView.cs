using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 화면의 탭 하나. Current는 표시용으로 읽기만 하고, 값 변경은 Modify로 Presenter에 맡긴다.
/// </summary>
public abstract class GameSettingsTabView : MonoBehaviour
{
    private GameSettingsPresenter _presenter;
    protected GameSettings Current => _presenter.Current;

    public void Bind(GameSettingsPresenter presenter)
    {
        _presenter = presenter;
        OnBind();
    }

    public abstract void Refresh();
    protected abstract void OnBind();

    protected void Modify(Action<GameSettings> change)
    {
        _presenter.Modify(change);
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