public interface IConnectView
{
    void Render(ConnectState state, string message);
    void ShowNickname(bool show);
}