using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.UI
{
    public class StartPanel : MonoBehaviour
    {
        public event Action SingleClicked;
        public event Action MultiClicked;
        public event Action BackClicked;

        [SerializeField] private GameObject _content;

        [SerializeField] private Button _singleButton;
        [SerializeField] private Button _multiButton;
        [SerializeField] private Button _backButton;

        private void Awake()
        {
            _singleButton.onClick.AddListener(() => SingleClicked?.Invoke());
            _multiButton.onClick.AddListener(() => MultiClicked?.Invoke());
            _backButton.onClick.AddListener(() => BackClicked?.Invoke());
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