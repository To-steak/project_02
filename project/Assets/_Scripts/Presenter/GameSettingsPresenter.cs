using System;

public class GameSettingsPresenter
{
    private readonly IGameSettingsView _view;

    public GameSettings Current { get; private set; }

    public GameSettingsPresenter(IGameSettingsView view)
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

    /// <summary>편집 중인 설정을 바꾸고 미리보기와 저장 버튼 상태를 갱신한다.</summary>
    public void Modify(Action<GameSettings> change)
    {
        change(Current);
        GameSettingsService.Instance.Preview(Current);
        UpdateSaveButton();
    }

    public void Save()
    {
        GameSettingsService.Instance.Save(Current);

        Current = GameSettingsService.Instance.Saved;
        UpdateSaveButton();
    }

    public void Reset()
    {
        Current = new GameSettings();
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
        bool dirty = !GameSettings.AreEqual(Current, GameSettingsService.Instance.Saved);
        _view.SetSaveInteractable(dirty);
    }
}