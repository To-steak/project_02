using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GameManager
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] Button[] header;
        [SerializeField] GameObject[] body;
        [SerializeField] SettingsTAB[] tabs;
        [SerializeField] Button save;
        [SerializeField] Button back;
        [SerializeField] Button reset;

        [SerializeField] AudioMixer mixer;
        const string MASTER = "MasterVolume";
        const string MUSIC = "MusicVolume";
        const string SFX = "SFXVolume";

        GameSettings _currentSettings;
        GameSettings _savedSettings;

        public GameSettings Current => _currentSettings;
        public bool IsDirty => !GameSettings.AreEqual(_currentSettings, _savedSettings);
        public event Action OnClose;

        const int DEFAULT_TAB_INDEX = 0;

        void Awake()
        {
            _savedSettings = SettingsIO.Load();
            _currentSettings = _savedSettings.Clone();

            foreach (var tab in tabs)
            {
                if (tab != null)
                {
                    tab.Bind(this);
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

            ApplyDisplay();
            RefreshSaveButton();
        }

        public void OpenSettings()
        {
            _currentSettings = _savedSettings.Clone();

            SelectTab(DEFAULT_TAB_INDEX);
            RefreshSaveButton();
        }

        public void OnSave()
        {
            ApplyDisplay();

            _savedSettings = _currentSettings.Clone();
            SettingsIO.Save(_savedSettings);
            RefreshSaveButton();
        }

        public void OnBack()
        {
            _currentSettings = _savedSettings.Clone();
            // ApplyPreview();
            OnClose?.Invoke();
        }

        public void OnReset()
        {
            _currentSettings = new GameSettings();

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
            FullScreenMode mode = _currentSettings.WindowMode switch
            {
                GameSettings.WindowModeType.ExclusiveFullScreen => FullScreenMode.ExclusiveFullScreen,
                GameSettings.WindowModeType.FullScreenWindow => FullScreenMode.FullScreenWindow,
                GameSettings.WindowModeType.Windowed => FullScreenMode.Windowed,
                _ => FullScreenMode.Windowed
            };

            int width = _currentSettings.ResolutionWidth > 0 ? _currentSettings.ResolutionWidth : Screen.width;
            int height = _currentSettings.ResolutionHeight > 0 ? _currentSettings.ResolutionHeight : Screen.height;

            if (Screen.width != width || Screen.height != height || Screen.fullScreenMode != mode)
            {
                Screen.SetResolution(width, height, mode);
            }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _currentSettings.TargetFrameRate;
        }

        private void ApplyPreview()
        {
            SetMixerVolume(MASTER, _currentSettings.MasterVolume);
            SetMixerVolume(MUSIC, _currentSettings.MusicVolume);
            SetMixerVolume(SFX, _currentSettings.SFXVolume);
        }

        private void SetMixerVolume(string param, float linear)
        {
            if (mixer == null || string.IsNullOrEmpty(param))
            {
                return;
            }

            // 볼륨은 로그 스케일이라 선형 값을 그대로 dB로 넣으면 체감이 이상하다.
            // 0은 log10에서 -무한대가 되므로 하한을 둔다. (-80dB = 믹서 최소값)
            float db = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
            mixer.SetFloat(param, db);
        }

        public void OnSettingsChanged()
        {
            // TODO: AudioMixer 연결 후 ApplyAudio 호출 복구
            // ApplyPreview();
            RefreshSaveButton();
        }

        private void RefreshSaveButton()
        {
            save.interactable = IsDirty;
        }
    }
}