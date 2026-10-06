using System;

public class GameSettingsPresenter : IDisposable
{
    private readonly IGameSettingsView _view;
    private readonly IGameSettingsService _service;

    public GameSettings Current { get; private set; }
    public bool IsDirty => !GameSettings.AreEqual(Current, _service.Saved);

    public event Action Closed;

    public GameSettingsPresenter(IGameSettingsView view, IGameSettingsService service)
    {
        _view = view;
        _service = service ?? throw new InvalidOperationException($"{nameof(IGameSettingsService)}가 등록되지 않았습니다.");

        _view.SaveRequested += OnSave;
        _view.ResetRequested += OnReset;
        _view.BackRequested += OnBack;

        Current = _service.Saved;
        UpdateSaveButton();
    }

    public void Open()
    {
        Current = _service.Saved;
        UpdateSaveButton();
    }

    public void NotifyChanged()
    {
        _service.Preview(Current);
        UpdateSaveButton();
    }

    private void OnSave()
    {
        _service.Save(Current);

        Current = _service.Saved;
        UpdateSaveButton();
    }

    private void OnBack()
    {
        Current = _service.Saved;
        _service.Preview(Current);
        Closed?.Invoke();
    }

    private void OnReset()
    {
        Current = new GameSettings();
        _service.Preview(Current);
        _view.RefreshSections();
        UpdateSaveButton();
    }

    private void UpdateSaveButton()
    {
        _view.SetSaveBtnInteractable(IsDirty);
    }

    public void Dispose()
    {
        _view.SaveRequested -= OnSave;
        _view.ResetRequested -= OnReset;
        _view.BackRequested -= OnBack;
    }
}
