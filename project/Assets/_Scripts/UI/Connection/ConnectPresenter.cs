using System;

public class ConnectPresenter : IDisposable
{
    private readonly IConnectView _view;
    private readonly IConnectionService _service;
    private bool _nicknameRequired;
    public event Action Back;

    public ConnectPresenter(IConnectView view, IConnectionService service)
    {
        _view = view;
        _service = service;

        _view.ConnectRequested += OnConnectRequested;
        _view.BackRequested += OnBackRequested;
        _service.StateChanged += OnStateChanged;

        _view.ShowNickname(false);
        _view.Render(ConnectState.Idle, null);
    }

    private void OnConnectRequested(string address, string nickname)
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

        _service.Connect(address, sendNickname);
    }

    private void OnStateChanged(ConnectState state)
    {
        if (state != ConnectState.Failed)
        {
            _view.Render(state, null);
            return;
        }

        var reason = _service.FailReason;
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

    private void OnBackRequested()
    {
        Back?.Invoke();
    }

    public void Dispose()
    {
        _view.ConnectRequested -= OnConnectRequested;
        _view.BackRequested -= OnBackRequested;
        _service.StateChanged -= OnStateChanged;
    }
}
