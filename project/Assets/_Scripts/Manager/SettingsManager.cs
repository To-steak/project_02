using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // MAIN MENU
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject settingsPanel;

    // SETTINGS
    [SerializeField] Button[] header;
    [SerializeField] GameObject[] body;
    [SerializeField] SettingsTAB[] tabs;
    [SerializeField] Button save;
    [SerializeField] Button back;
    [SerializeField] Button reset;

    // AUDIO
    [SerializeField] AudioMixer mixer;
    [SerializeField] string masterParam = "MasterVolume";
    [SerializeField] string musicParam = "MusicVolume";
    [SerializeField] string sfxParam = "SFXVolume";

    GameSettings _current;
    GameSettings _saved;

    public GameSettings Current => _current;
    public bool IsDirty => !GameSettings.AreEqual(_current, _saved);

    void Awake()
    {
        _saved = SettingsIO.Load();
        _current = _saved.Clone();

        foreach (var item in tabs)
        {
            if (item != null)
            {
                item.Bind(this);
            }
        }

        for (int i = 0; i < header.Length; i++)
        {
            int index = i;
            header[i].onClick.AddListener(() => SelectTab(index));
        }
        save.onClick.AddListener(OnSave);
        back.onClick.AddListener(OnBack);
        reset.onClick.AddListener(OnReset);

        ApplyPreview();
        ApplyDisplay();
        RefreshSaveButton();

        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        _current = _saved.Clone();
        ApplyPreview();

        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        SelectTab(0);
        RefreshSaveButton();
    }

    public void OnSave()
    {
        ApplyDisplay();

        _saved = _current.Clone();
        SettingsIO.Save(_saved);
        RefreshSaveButton();

    }

    public void OnBack()
    {
        _current = _saved.Clone();
        ApplyPreview();

        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OnReset()
    {
        _current = new GameSettings();
        ApplyPreview();

        foreach (var tab in tabs)
        {
            if (tab != null)
            {
                tab.Refresh();
            }
        }

        RefreshSaveButton();
    }



    private void SelectTab(int index)
    {
        for (int i = 0; i < body.Length; i++)
        {
            body[i].SetActive(i == index);
        }

        if (index >= 0 && index < tabs.Length && tabs[index] != null)
        {
            tabs[index].Refresh();
        }
    }

    private void ApplyDisplay()
    {
        FullScreenMode mode = _current.WindowMode switch
        {
            GameSettings.WindowModeType.ExclusiveFullScreen => FullScreenMode.ExclusiveFullScreen,
            GameSettings.WindowModeType.FullScreenWindow => FullScreenMode.FullScreenWindow,
            GameSettings.WindowModeType.Windowed => FullScreenMode.Windowed,
            _ => FullScreenMode.Windowed
        };

        int width = _current.ResolutionWidth > 0 ? _current.ResolutionWidth : Screen.width;
        int height = _current.ResolutionHeight > 0 ? _current.ResolutionHeight : Screen.height;

        if (Screen.width != width || Screen.height != height || Screen.fullScreenMode != mode)
        {
            Screen.SetResolution(width, height, mode);
        }
        
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _current.TargetFrameRate;
    }

    private void ApplyPreview()
    {
        SetMixerVolume(masterParam, _current.MasterVolume);
        SetMixerVolume(musicParam, _current.MusicVolume);
        SetMixerVolume(sfxParam, _current.SFXVolume);
    }

    private void SetMixerVolume(string param, float linear)
    {
        if (mixer == null || string.IsNullOrEmpty(param))
        {
            return;
        }

        float db = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        mixer.SetFloat(param, db);
    }

    public void OnSettingsChanged()
    {
        ApplyPreview();
        RefreshSaveButton();
    }

    private void RefreshSaveButton()
    {
        save.interactable = IsDirty;
    }
}
