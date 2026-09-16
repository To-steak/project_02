using UnityEngine.UI;
using UnityEngine;

public class AudioTAB : SettingsTAB
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

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

        masterSlider.onValueChanged.AddListener(volume => { Current.MasterVolume = volume; NotifyChanged(); });
        musicSlider.onValueChanged.AddListener(volume => { Current.MusicVolume = volume; NotifyChanged(); });
        sfxSlider.onValueChanged.AddListener(volume => { Current.SFXVolume = volume; NotifyChanged(); });
    }

    public override void Refresh()
    {
        masterSlider.SetValueWithoutNotify(Current.MasterVolume);
        musicSlider.SetValueWithoutNotify(Current.MusicVolume);
        sfxSlider.SetValueWithoutNotify(Current.SFXVolume);
    }
}