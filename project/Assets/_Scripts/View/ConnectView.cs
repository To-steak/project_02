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
    [SerializeField] private TMP_InputField _nicknameInput;

    /// <summary>뒤로가기로 화면을 닫았다. 화면 전환은 상위 View(MainMenuView)가 처리한다.</summary>
    public event Action Closed;

    private ConnectPresenter _presenter;

    private void Awake()
    {
        _nicknameInput.characterLimit = NicknameRule.MAX_LENGTH;

        _presenter = new ConnectPresenter(this);

        _connectButton.onClick.AddListener(() => _presenter.Connect(_addressInput.text, _nicknameInput.text));
        _backButton.onClick.AddListener(() => Closed?.Invoke());
    }

    private void OnDestroy()
    {
        _presenter?.Dispose();
    }

    public void ShowNickname(bool show)
    {
        _nicknameInput.gameObject.SetActive(show);
        if (show)
        {
            _nicknameInput.Select();
        }
    }

    public void Render(ConnectState state, string message)
    {
        bool idle = state != ConnectState.Connecting && state != ConnectState.Connected;
        _connectButton.interactable = idle;
        _backButton.interactable = idle;
        _addressInput.interactable = idle;
        _nicknameInput.interactable = idle;

        _statusText.text = state switch
        {
            ConnectState.Connecting => "connecting",
            ConnectState.Failed => message ?? "can't connect to server",
            _ => ""
        };
    }
}