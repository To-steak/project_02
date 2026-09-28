using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ConnectView : MonoBehaviour, IConnectView
{
    [SerializeField] private TMP_InputField _addressInput;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _statusText;

    public event Action<string> ConnectRequested;
    public event Action BackRequested;
    public event Action OnBack;

    private ConnectPresenter _presenter;

    private void Awake()
    {
        _connectButton.onClick.AddListener(() => ConnectRequested?.Invoke(_addressInput.text));
        _backButton.onClick.AddListener(() => BackRequested?.Invoke());

        _presenter = new ConnectPresenter(this, GameServices.Connection);
        _presenter.Back += () => OnBack?.Invoke();
    }

    private void OnDestroy() => _presenter?.Dispose();

    public void Render(ConnectState state)
    {
        bool idle = state != ConnectState.Connecting && state != ConnectState.Connected;
        _connectButton.interactable = idle;
        _backButton.interactable = idle;
        _addressInput.interactable = idle;

        _statusText.text = state switch
        {
            ConnectState.Connecting => "connecting",
            ConnectState.Failed => "can't connect to server",
            _ => ""
        };
    }
}
