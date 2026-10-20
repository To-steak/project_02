using System;
using System.Collections.Generic;
using UnityEngine;
using Project02.Service;

namespace Project02.UI
{
    public class SettingsPresenter : MonoBehaviour
    {
        public event Action Closed;

        [SerializeField] private SettingsPanel _view;

        [Header("Graphics")]
        [SerializeField] private DropdownWidget _resolution;
        [SerializeField] private DropdownWidget _windowMode;
        [SerializeField] private DropdownWidget _frameRate;

        [Header("Audio")]
        [SerializeField] private SliderWidget _masterVolume;
        [SerializeField] private SliderWidget _musicVolume;
        [SerializeField] private SliderWidget _sfxVolume;

        [Header("Control")]
        [SerializeField] private SliderWidget _mouseSensitivity;
        [SerializeField] private KeyBindingWidget _keyBinding;

        [Header("Language")]
        [SerializeField] private DropdownWidget _language;

        private const int VOLUME_MIN = 0;
        private const int VOLUME_MAX = 100;
        private const float VOLUME_SCALE = 100f;
        private const int SENSITIVITY_MIN = 1;
        private const int SENSITIVITY_MAX = 100;
        private const float SENSITIVITY_SCALE = 100f;

        private static readonly int[] FRAME_RATES = { 60, 120, 144, -1 };   // -1 = 제한 없음
        private static readonly string[] WINDOW_MODE_LABELS = { "Full", "Borderless", "Windowed" };
        private static readonly string[] LOCALE_CODES = { "en", "ko" };
        private static readonly string[] LOCALE_LABELS = { "English", "한국어" };

        private readonly List<Vector2Int> _resolutions = new();
        private GameSettings _current;

        private void Awake()
        {
            SetupResolutions();
            _windowMode.SetOptions(new List<string>(WINDOW_MODE_LABELS));
            _frameRate.SetOptions(FrameRateLabels());
            _language.SetOptions(new List<string>(LOCALE_LABELS));

            _masterVolume.SetRange(VOLUME_MIN, VOLUME_MAX);
            _musicVolume.SetRange(VOLUME_MIN, VOLUME_MAX);
            _sfxVolume.SetRange(VOLUME_MIN, VOLUME_MAX);
            _mouseSensitivity.SetRange(SENSITIVITY_MIN, SENSITIVITY_MAX);
        }

        private void OnEnable()
        {
            _view.SaveClicked += OnSave;
            _view.ResetClicked += OnReset;
            _view.BackClicked += OnBack;

            _resolution.Changed += OnResolutionChanged;
            _windowMode.Changed += OnWindowModeChanged;
            _frameRate.Changed += OnFrameRateChanged;
            _masterVolume.Changed += OnMasterVolumeChanged;
            _musicVolume.Changed += OnMusicVolumeChanged;
            _sfxVolume.Changed += OnSfxVolumeChanged;
            _mouseSensitivity.Changed += OnMouseSensitivityChanged;
            _keyBinding.Changed += OnKeyBindingChanged;
            _language.Changed += OnLanguageChanged;
        }

        private void OnDisable()
        {
            _view.SaveClicked -= OnSave;
            _view.ResetClicked -= OnReset;
            _view.BackClicked -= OnBack;

            _resolution.Changed -= OnResolutionChanged;
            _windowMode.Changed -= OnWindowModeChanged;
            _frameRate.Changed -= OnFrameRateChanged;
            _masterVolume.Changed -= OnMasterVolumeChanged;
            _musicVolume.Changed -= OnMusicVolumeChanged;
            _sfxVolume.Changed -= OnSfxVolumeChanged;
            _mouseSensitivity.Changed -= OnMouseSensitivityChanged;
            _keyBinding.Changed -= OnKeyBindingChanged;
            _language.Changed -= OnLanguageChanged;
        }

        public void Open()
        {
            _current = SettingsService.Instance.Saved;
            _view.Show();
            Render();
            UpdateSaveButton();
        }

        private void Render()
        {
            _resolution.Render(FindResolutionIndex(_current.ResolutionWidth, _current.ResolutionHeight));
            _windowMode.Render((int)_current.WindowMode);
            _frameRate.Render(Mathf.Max(0, Array.IndexOf(FRAME_RATES, _current.TargetFrameRate)));

            _masterVolume.Render(Mathf.RoundToInt(_current.MasterVolume * VOLUME_SCALE));
            _musicVolume.Render(Mathf.RoundToInt(_current.MusicVolume * VOLUME_SCALE));
            _sfxVolume.Render(Mathf.RoundToInt(_current.SFXVolume * VOLUME_SCALE));

            int sensitivity = Mathf.RoundToInt(_current.MouseSensitivity * SENSITIVITY_SCALE);
            _mouseSensitivity.Render(Mathf.Clamp(sensitivity, SENSITIVITY_MIN, SENSITIVITY_MAX));
            _keyBinding.Render();

            _language.Render(Mathf.Max(0, Array.IndexOf(LOCALE_CODES, _current.LocaleCode)));
        }

        private void OnResolutionChanged(int index)
        {
            _current.ResolutionWidth = _resolutions[index].x;
            _current.ResolutionHeight = _resolutions[index].y;
            Apply();
        }

        private void OnWindowModeChanged(int index)
        {
            _current.WindowMode = (WindowModeType)index;
            Apply();
        }

        private void OnFrameRateChanged(int index)
        {
            _current.TargetFrameRate = FRAME_RATES[index];
            Apply();
        }

        private void OnMasterVolumeChanged(int value)
        {
            _current.MasterVolume = value / VOLUME_SCALE;
            Apply();
        }

        private void OnMusicVolumeChanged(int value)
        {
            _current.MusicVolume = value / VOLUME_SCALE;
            Apply();
        }

        private void OnSfxVolumeChanged(int value)
        {
            _current.SFXVolume = value / VOLUME_SCALE;
            Apply();
        }

        private void OnMouseSensitivityChanged(int value)
        {
            _current.MouseSensitivity = value / SENSITIVITY_SCALE;
            Apply();
        }

        private void OnKeyBindingChanged(string json)
        {
            _current.KeyBinding = json;
            Apply();
        }

        private void OnLanguageChanged(int index)
        {
            _current.LocaleCode = LOCALE_CODES[index];
            Apply();
        }


        private void OnSave()
        {
            SettingsService.Instance.Save(_current);
            UpdateSaveButton();
        }

        private void OnReset()
        {
            _current = default;
            SettingsService.Instance.Preview(_current);
            Render();
            UpdateSaveButton();
        }

        private void OnBack()
        {
            _current = SettingsService.Instance.Saved;
            SettingsService.Instance.Preview(_current);
            _view.Hide();
            Closed?.Invoke();
        }

        private void Apply()
        {
            SettingsService.Instance.Preview(_current);
            UpdateSaveButton();
        }

        private void UpdateSaveButton()
        {
            _view.SetSaveInteractable(!_current.Equals(SettingsService.Instance.Saved));
        }

        private void SetupResolutions()
        {
            HashSet<Vector2Int> seen = new();
            List<string> labels = new();
            foreach (var resolution in Screen.resolutions)
            {
                Vector2Int size = new(resolution.width, resolution.height);
                if (seen.Add(size))
                {
                    _resolutions.Add(size);
                    labels.Add($"{size.x} x {size.y}");
                }
            }

            _resolution.SetOptions(labels);
        }

        private int FindResolutionIndex(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                width = Screen.width;
                height = Screen.height;
            }

            int index = _resolutions.IndexOf(new Vector2Int(width, height));
            return index >= 0 ? index : Mathf.Max(0, _resolutions.Count - 1);
        }

        private static List<string> FrameRateLabels()
        {
            List<string> labels = new();
            foreach (int fps in FRAME_RATES)
            {
                labels.Add(fps < 0 ? "inf" : $"{fps} FPS");
            }
            return labels;
        }
    }
}