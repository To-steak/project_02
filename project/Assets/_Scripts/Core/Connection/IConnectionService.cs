using System;

[Serializable]
public class ConnectPayload
{
    public string UserId;
    public string Nickname;
}

public enum ConnectState
{
    Idle, Connecting, Connected, Failed, Disconnected
}

public enum ConnectReject
{
    None = 0, // 거절이 아님 (서버에 닿지 못했거나, 접속 중에 끊김)
    InvalidPayload,
    AlreadyConnected,
    NicknameRequired,
    NicknameInvalid,
    NicknameTaken,
}

public interface IConnectionService
{
    event Action<ConnectState> StateChanged;
    void Connect(string address, string nickname);
    ConnectReject FailReason { get; }
}
