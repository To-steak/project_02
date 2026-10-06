using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SettingsService : MonoBehaviour, ISettingsService
{
    [SerializeField] private AudioMixer _mixer;

    private const string MASTER = "MasterVolume";
    private const string MUSIC = "MusicVolume";
    private const string SFX = "SFXVolume";
    private const float MIN_SENSITIVITY = 0.01f;
    private const float MAX_SENSITIVITY = 1f;

    private GameSettings _saved;
    public GameSettings Saved => _saved.Clone();

    private void Awake()
    {
        if (GameServices.Settings != null)
        {
            Destroy(this);
            return;
        }

        _saved = SettingsStore.Load();
        GameServices.Register(this);
    }

    private void Start()
    {
        if (Application.isBatchMode)
        {
            return;
        }
        
        // AudioMixer.SetFloat은 Awake에서 호출하면 무시되는 경우가 있어서 Start에서 적용한다.
        ApplyDisplay(_saved);
        Preview(_saved);
    }

    private void OnDestroy()
    {
        GameServices.Unregister(this);
    }

    public void Preview(GameSettings settings)
    {
        SetMixerVolume(MASTER, settings.MasterVolume);
        SetMixerVolume(MUSIC, settings.MusicVolume);
        SetMixerVolume(SFX, settings.SFXVolume);

        InputService.MouseSensitivity = Mathf.Clamp(settings.MouseSensitivity, MIN_SENSITIVITY, MAX_SENSITIVITY);
        SetKeyboardBinding(settings.KeyBinding);
    }

    public void Save(GameSettings settings)
    {
        _saved = settings.Clone();
        SettingsStore.Save(_saved);

        ApplyDisplay(_saved);
        Preview(_saved);
    }

    private static void ApplyDisplay(GameSettings settings)
    {
        FullScreenMode mode = settings.WindowMode switch
        {
            GameSettings.WindowModeType.ExclusiveFullScreen => FullScreenMode.ExclusiveFullScreen,
            GameSettings.WindowModeType.FullScreenWindow => FullScreenMode.FullScreenWindow,
            GameSettings.WindowModeType.Windowed => FullScreenMode.Windowed,
            _ => FullScreenMode.Windowed
        };

        int width = settings.ResolutionWidth > 0 ? settings.ResolutionWidth : Screen.width;
        int height = settings.ResolutionHeight > 0 ? settings.ResolutionHeight : Screen.height;

        if (Screen.width != width || Screen.height != height || Screen.fullScreenMode != mode)
        {
            Screen.SetResolution(width, height, mode);
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = settings.TargetFrameRate;
    }

    private void SetMixerVolume(string param, float linear)
    {
        if (_mixer == null)
        {
            return;
        }

        // 볼륨은 로그 스케일이라 선형 값을 그대로 dB로 넣으면 체감이 이상하다.
        // 0은 log10에서 -무한대가 되므로 하한을 둔다. (-80dB = 믹서 최소값)
        float db = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        _mixer.SetFloat(param, db);
    }

    private static void SetKeyboardBinding(string json)
    {
        var asset = InputService.Actions.asset;
        if (string.IsNullOrEmpty(json))
        {
            asset.RemoveAllBindingOverrides();
        }
        else
        {
            asset.LoadBindingOverridesFromJson(json);
        }
    }
}
