using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class AudioSettingsView : GameSettingsTabView
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [SerializeField] TMP_Text masterLabel;
    [SerializeField] TMP_Text musicLabel;
    [SerializeField] TMP_Text sfxLabel;

    const int MIN_AUDIO_VOLUME = 0;
    const int MAX_AUDIO_VOLUME = 100;
    const float VOLUME_SCALE = 100f;

    protected override void OnBind()
    {
        SetupSlider(masterSlider, MIN_AUDIO_VOLUME, MAX_AUDIO_VOLUME);
        SetupSlider(musicSlider, MIN_AUDIO_VOLUME, MAX_AUDIO_VOLUME);
        SetupSlider(sfxSlider, MIN_AUDIO_VOLUME, MAX_AUDIO_VOLUME);

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    public override void Refresh()
    {
        float master = Current.MasterVolume * VOLUME_SCALE;
        float music = Current.MusicVolume * VOLUME_SCALE;
        float sfx = Current.SFXVolume * VOLUME_SCALE;

        masterSlider.SetValueWithoutNotify(master);
        musicSlider.SetValueWithoutNotify(music);
        sfxSlider.SetValueWithoutNotify(sfx);

        UpdateLabel(masterLabel, master);
        UpdateLabel(musicLabel, music);
        UpdateLabel(sfxLabel, sfx);
    }

    private void OnMasterChanged(float value)
    {
        Modify(s => s.MasterVolume = value / VOLUME_SCALE);
        UpdateLabel(masterLabel, value);
    }

    private void OnMusicChanged(float value)
    {
        Modify(s => s.MusicVolume = value / VOLUME_SCALE);
        UpdateLabel(musicLabel, value);
    }

    private void OnSFXChanged(float value)
    {
        Modify(s => s.SFXVolume = value / VOLUME_SCALE);
        UpdateLabel(sfxLabel, value);
    }
}