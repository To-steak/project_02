using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class AudioTAB : SettingsTAB
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [SerializeField] TMP_Text masterLabel;
    [SerializeField] TMP_Text musicLabel;
    [SerializeField] TMP_Text sfxLabel;

    const float VOLUME_SCALE = 100f;

    static void SetupSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    protected override void OnBind()
    {
        SetupSlider(masterSlider);
        SetupSlider(musicSlider);
        SetupSlider(sfxSlider);

        masterSlider.onValueChanged.AddListener(volume =>
        {
            Current.MasterVolume = volume;
            UpdateLabel(masterLabel, volume);
            NotifyChanged();
        });
        musicSlider.onValueChanged.AddListener(volume =>
        {
            Current.MusicVolume = volume;
            UpdateLabel(musicLabel, volume);
            NotifyChanged();
        });
        sfxSlider.onValueChanged.AddListener(volume =>
        {
            Current.SFXVolume = volume;
            UpdateLabel(sfxLabel, volume);
            NotifyChanged();
        });
    }

    public override void Refresh()
    {
        masterSlider.SetValueWithoutNotify(Current.MasterVolume);
        musicSlider.SetValueWithoutNotify(Current.MusicVolume);
        sfxSlider.SetValueWithoutNotify(Current.SFXVolume);

        UpdateLabel(masterLabel, Current.MasterVolume);
        UpdateLabel(musicLabel, Current.MusicVolume);
        UpdateLabel(sfxLabel, Current.SFXVolume);
    }

    static void UpdateLabel(TMP_Text label, float volume)
    {
        if (label != null)
        {
            label.text = Mathf.RoundToInt(volume * VOLUME_SCALE).ToString();
        }
    }
}