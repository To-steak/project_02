using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.View
{
    public class ConnectView : MonoBehaviour
    {
        public event Action<string, string> ConnectClicked; // address, nickname
        public event Action BackClicked;

        [SerializeField] private GameObject _content;

        [SerializeField] private TMP_InputField _addressInput;
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _statusText;

        private void Awake()
        {
            _nicknameInput.characterLimit = NicknameRule.MAX_LENGTH;

            _connectButton.onClick.AddListener(() => ConnectClicked?.Invoke(_addressInput.text, _nicknameInput.text));
            _backButton.onClick.AddListener(() => BackClicked?.Invoke());
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

        public void Show()
        {
            _content.SetActive(true);
        }

        public void Hide()
        {
            _content.SetActive(false);
        }
    }
}