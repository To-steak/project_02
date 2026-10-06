using System;

public enum ConnectState
{
    Idle, Connecting, Connected, Failed, Disconnected
}

public interface IConnectionService
{
    event Action<ConnectState> StateChanged;
    void Connect(string address, string nickname);
    ConnectReject FailReason { get; }
}
