using System;

public interface IConnectView
{
    event Action<string, string> ConnectRequested; // address, nickname
    event Action BackRequested;
    void Render(ConnectState state, string message);
    void ShowNickname(bool show);
}
