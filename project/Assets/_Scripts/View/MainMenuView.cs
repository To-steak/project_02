using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.View
{
    public class MainMenuView : MonoBehaviour
    {
        public event Action StartClicked;
        public event Action SettingsClicked;
        public event Action ExitClicked;

        [SerializeField] private GameObject _content;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private void Awake()
        {
            _startButton.onClick.AddListener(() => StartClicked?.Invoke());
            _settingsButton.onClick.AddListener(() => SettingsClicked?.Invoke());
            _exitButton.onClick.AddListener(() => ExitClicked?.Invoke());
        }

        public void Show()
        {
            _content.SetActive(true);
        }

        public void Hide()
        {
            _content.SetActive(false);
        }
    }
}