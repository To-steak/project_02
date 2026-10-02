using System;

public interface ISettingsView
{
    event Action SaveRequested;
    event Action ResetRequested;
    event Action BackRequested;

    void SetSaveBtnInteractable(bool value);
    void RefreshSections();
}