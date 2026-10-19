using System;
using Project02.Service;
using Project02.View;
using UnityEngine;

namespace Project02.Presenter
{
    public class ConnectPresenter : MonoBehaviour
    {
        public event Action Closed;

        [SerializeField] private ConnectView _view;

        private ConnectionService _service;
        private bool _nicknameRequired;

        private void OnEnable()
        {
            _service = ConnectionService.Instance;

            _view.ConnectClicked += OnConnectClicked;
            _view.BackClicked += OnBackClicked;
            _service.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            _view.ConnectClicked -= OnConnectClicked;
            _view.BackClicked -= OnBackClicked;
            _service.StateChanged -= OnStateChanged;
        }

        public void Open()
        {
            _nicknameRequired = false;
            _view.Show();
            _view.ShowNickname(false);
            _view.Render(ConnectState.Idle, null);
        }

        private void OnConnectClicked(string address, string nickname)
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

            ConnectReject reason = _service.FailReason;
            if (reason == ConnectReject.NicknameRequired)
            {
                _nicknameRequired = true;
                _view.ShowNickname(true);
            }

            _view.Render(state, MessageFor(reason));
        }

        private void OnBackClicked()
        {
            _view.Hide();
            Closed?.Invoke();
        }

        private static string MessageFor(ConnectReject reason) => reason switch
        {
            ConnectReject.NicknameRequired => "first connection: choose a nickname",
            ConnectReject.NicknameInvalid => "invalid nickname",
            ConnectReject.NicknameTaken => "nickname already taken",
            ConnectReject.AlreadyConnected => "already connected",
            _ => null
        };
    }
}