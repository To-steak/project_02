using System;

public interface IGameSettingsView
{
    event Action SaveRequested;
    event Action ResetRequested;
    event Action BackRequested;

    void SetSaveBtnInteractable(bool value);
    void RefreshSections();
}