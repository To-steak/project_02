using System;

public class ConnectPresenter : IDisposable
{
    private readonly IConnectView _view;
    private readonly IConnectionService _service;
    public event Action Back;

    public ConnectPresenter(IConnectView view, IConnectionService service)
    {
        _view = view;
        _service = service ?? throw new InvalidOperationException($"{nameof(IConnectionService)}가 등록되지 않았습니다.");

        _view.ConnectRequested += OnConnectRequested;
        _view.BackRequested += OnBackRequested;
        _service.StateChanged += _view.Render;

        _view.Render(ConnectState.Idle);
    }

    private void OnConnectRequested(string address)
    {
        address = address?.Trim();
        if (string.IsNullOrEmpty(address))
        {
            _view.Render(ConnectState.Failed);
            return;
        }

        _service.Connect(address);
    }

    private void OnBackRequested()
    {
        Back?.Invoke();
    }

    public void Dispose()
    {
        _view.ConnectRequested -= OnConnectRequested;
        _view.BackRequested -= OnBackRequested;
        _service.StateChanged -= _view.Render;
    }
}
