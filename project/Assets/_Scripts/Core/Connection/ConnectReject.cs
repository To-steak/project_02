public enum ConnectReject
{
    None = 0, // 거절이 아님 (서버에 닿지 못했거나, 접속 중에 끊김)
    InvalidPayload,
    AlreadyConnected,
    NicknameRequired,
    NicknameInvalid,
    NicknameTaken,
}