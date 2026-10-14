using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
namespace Project02.Service
{
    public enum WindowModeType
    {
        ExclusiveFullScreen = 0,
        FullScreenWindow = 1,
        Windowed = 2
    }

    [Serializable]
    public struct GameSettings
    {
        // Graphic
        public int ResolutionWidth;
        public int ResolutionHeight;
        public WindowModeType WindowMode;
        public int TargetFrameRate;

        // Audio
        public float MasterVolume;
        public float MusicVolume;
        public float SFXVolume;

        // Keyboard
        public float MouseSensitivity;
        public string KeyBinding;

        // Language
        public string LocaleCode;
    }

    public class GameSettingsService : MonoBehaviour
    {
        public static GameSettingsService Instance { get; private set; }

        [SerializeField] private AudioMixer _mixer;

        private const string SETTINGS_FILE = "settings.json";
        private const string MASTER = "MasterVolume";
        private const string MUSIC = "MusicVolume";
        private const string SFX = "SFXVolume";
        private const float MIN_SENSITIVITY = 0.01f;
        private const float MAX_SENSITIVITY = 1f;

        private string _path;
        private GameSettings _saved;
        public GameSettings Saved => _saved;

        private void Awake()
        {
            _path = Path.Combine(Application.persistentDataPath, SETTINGS_FILE);
            _saved = Load();

            Instance = this;
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
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Preview(GameSettings settings)
        {
            SetMixerVolume(MASTER, settings.MasterVolume);
            SetMixerVolume(MUSIC, settings.MusicVolume);
            SetMixerVolume(SFX, settings.SFXVolume);

            InputService.Instance.MouseSensitivity = Mathf.Clamp(settings.MouseSensitivity, MIN_SENSITIVITY, MAX_SENSITIVITY);
            SetKeyboardBinding(settings.KeyBinding);
        }

        public void Save(GameSettings settings)
        {
            _saved = settings;
            JsonFile.TrySave(_path, _saved);

            ApplyDisplay(_saved);
            Preview(_saved);
        }

        private GameSettings Load()
        {
            return JsonFile.TryLoad(_path, out GameSettings settings) ? settings : default;
        }

        private static void ApplyDisplay(GameSettings settings)
        {
            FullScreenMode mode = settings.WindowMode switch
            {
                WindowModeType.ExclusiveFullScreen => FullScreenMode.ExclusiveFullScreen,
                WindowModeType.FullScreenWindow => FullScreenMode.FullScreenWindow,
                WindowModeType.Windowed => FullScreenMode.Windowed,
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
            var asset = InputService.Instance.Actions.asset;
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
}