using System;

public enum ConnectState
{
    Idle, Connecting, Connected, Failed
}

public interface IConnectionService
{
    event Action<ConnectState> StateChanged;
    void Connect(string address);
}
