using System;

public class ConnectPresenter : IDisposable
{
    private readonly IConnectView _view;
    private bool _nicknameRequired;

    public ConnectPresenter(IConnectView view)
    {
        _view = view;

        ConnectionService.Instance.StateChanged += OnStateChanged;

        _view.ShowNickname(false);
        _view.Render(ConnectState.Idle, null);
    }

    public void Connect(string address, string nickname)
    {
        address = address?.Trim();
        if (string.IsNullOrEmpty(address))
        {
            _view.Render(ConnectState.Failed, "enter server address");
            return;
        }

        string sendNickname = null;
        if (_nicknameRequired && !NicknameRule.TryNormalize(nickname, out sendNickname, out string error))
        {
            _view.Render(ConnectState.Failed, error);
            return;
        }

        ConnectionService.Instance.Connect(address, sendNickname);
    }

    private void OnStateChanged(ConnectState state)
    {
        if (state != ConnectState.Failed)
        {
            _view.Render(state, null);
            return;
        }

        var reason = ConnectionService.Instance.FailReason;
        if (reason == ConnectReject.NicknameRequired)
        {
            _nicknameRequired = true;
            _view.ShowNickname(true);
        }

        _view.Render(state, MessageFor(reason));
    }

    private static string MessageFor(ConnectReject reason) => reason switch
    {
        ConnectReject.NicknameRequired => "first connection: choose a nickname",
        ConnectReject.NicknameInvalid => "invalid nickname",
        ConnectReject.NicknameTaken => "nickname already taken",
        ConnectReject.AlreadyConnected => "already connected",
        _ => null
    };

    public void Dispose()
    {
        ConnectionService.Instance.StateChanged -= OnStateChanged;
    }
}