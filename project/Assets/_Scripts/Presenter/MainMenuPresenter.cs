using Project02.View;
using UnityEngine;

namespace Project02.Presenter
{
    public class MainMenuPresenter : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;
        [SerializeField] private StartPresenter _start;
        [SerializeField] private SettingsPresenter _settings;

        private void OnEnable()
        {
            _view.StartClicked += OnStartClicked;
            _view.SettingsClicked += OnSettingsClicked;
            _view.ExitClicked += OnExitClicked;

            _start.Closed += OnStartClosed;
            _settings.Closed += OnSettingsClosed;
        }

        private void OnDisable()
        {
            _view.StartClicked -= OnStartClicked;
            _view.SettingsClicked -= OnSettingsClicked;
            _view.ExitClicked -= OnExitClicked;

            _start.Closed -= OnStartClosed;
            _settings.Closed -= OnSettingsClosed;
        }
        
        private void Start()
        {
            _view.Show();
        }

        private void OnStartClicked()
        {
            _view.Hide();
            _start.Open();
        }

        private void OnSettingsClicked()
        {
            _view.Hide();
            _settings.Open();
        }

        private void OnStartClosed()
        {
            _view.Show();
        }

        private void OnSettingsClosed()
        {
            _view.Show();
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}