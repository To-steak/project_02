using System;
using Project02.View;
using UnityEngine;

namespace Project02.Presenter
{
    public class StartPresenter : MonoBehaviour
    {
        public event Action Closed;

        [SerializeField] private StartView _view;
        [SerializeField] private ConnectPresenter _connect;

        private void OnEnable()
        {
            _view.SingleClicked += OnSingleClicked;
            _view.MultiClicked += OnMultiClicked;
            _view.BackClicked += OnBackClicked;
            _connect.Closed += OnConnectClosed;
        }

        private void OnDisable()
        {
            _view.SingleClicked -= OnSingleClicked;
            _view.MultiClicked -= OnMultiClicked;
            _view.BackClicked -= OnBackClicked;
            _connect.Closed -= OnConnectClosed;
        }

        public void Open()
        {
            _view.Show();
        }

        private void OnSingleClicked()
        {
            // TODO: 싱글 플레이 시작
        }

        private void OnMultiClicked()
        {
            _view.Hide();
            _connect.Open();
        }

        private void OnConnectClosed()
        {
            _view.Show();
        }

        private void OnBackClicked()
        {
            _view.Hide();
            Closed?.Invoke();
        }
    }
}