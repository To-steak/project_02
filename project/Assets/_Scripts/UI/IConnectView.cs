using System;

public interface IConnectView
{
    event Action<string> ConnectRequested;
    event Action BackRequested;
    void Render(ConnectState state);
}
