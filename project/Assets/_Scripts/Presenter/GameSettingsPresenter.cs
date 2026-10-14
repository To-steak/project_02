using System;
using Project02.Service;

namespace Project02.Presenter
{
    public class GameSettingsPresenter
    {
        public interface IView
        {
            event Action SaveClicked;
            event Action ResetClicked;
            event Action BackClicked;

            void Show();
            void Hide();
            void SetSaveInteractable(bool value);
            void RefreshTabs();
        }

        public event Action Closed;

        public GameSettings Current { get; private set; }

        private readonly IView _view;

        public GameSettingsPresenter(IView view)
        {
            _view = view;

            Current = GameSettingsService.Instance.Saved;
            UpdateSaveButton();
        }

        public void Open()
        {
            Current = GameSettingsService.Instance.Saved;
            UpdateSaveButton();
        }

        public void Apply(GameSettings next)
        {
            Current = next;
            GameSettingsService.Instance.Preview(Current);
            UpdateSaveButton();
        }

        public void Save()
        {
            GameSettingsService.Instance.Save(Current);
            UpdateSaveButton();
        }

        public void Reset()
        {
            Current = default;
            GameSettingsService.Instance.Preview(Current);
            _view.RefreshTabs();
            UpdateSaveButton();
        }

        public void Cancel()
        {
            Current = GameSettingsService.Instance.Saved;
            GameSettingsService.Instance.Preview(Current);
        }

        private void UpdateSaveButton()
        {
            bool dirty = !Current.Equals(GameSettingsService.Instance.Saved);
            _view.SetSaveInteractable(dirty);
        }
    }
}