using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        public event Action SaveClicked;
        public event Action ResetClicked;
        public event Action BackClicked;

        [SerializeField] private GameObject _content;
        [SerializeField] private Button[] _pageButtons;
        [SerializeField] private GameObject[] _pages;
        [SerializeField] private Button _save;
        [SerializeField] private Button _reset;
        [SerializeField] private Button _back;

        private const int DEFAULT_PAGE = 0;

        private void Awake()
        {
            Debug.Assert(_pageButtons.Length == _pages.Length, $"{name}: 페이지 버튼과 페이지 개수가 다름", this);

            for (int i = 0; i < _pageButtons.Length; i++)
            {
                int index = i;
                _pageButtons[i].onClick.AddListener(() => SelectPage(index));
            }

            _save.onClick.AddListener(() => SaveClicked?.Invoke());
            _reset.onClick.AddListener(() => ResetClicked?.Invoke());
            _back.onClick.AddListener(() => BackClicked?.Invoke());
        }

        public void Show()
        {
            _content.SetActive(true);
            SelectPage(DEFAULT_PAGE);
        }

        public void Hide()
        {
            _content.SetActive(false);
        }

        public void SetSaveInteractable(bool value)
        {
            _save.interactable = value;
        }

        private void SelectPage(int index)
        {
            for (int i = 0; i < _pages.Length; i++)
            {
                _pages[i].SetActive(i == index);
            }
        }
    }
}