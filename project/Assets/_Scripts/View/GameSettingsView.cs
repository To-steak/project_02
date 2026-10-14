using System;
using Project02.Presenter;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.View
{
    public class GameSettingsView : MonoBehaviour, GameSettingsPresenter.IView
    {
        public event Action SaveClicked;
        public event Action ResetClicked;
        public event Action BackClicked;

        [SerializeField] Button[] header;
        [SerializeField] GameSettingsTabView[] tabs;
        [SerializeField] Button save;
        [SerializeField] Button back;
        [SerializeField] Button reset;

        private const int DEFAULT_TAB_INDEX = 0;
        private GameSettingsPresenter _presenter;

        private void Awake()
        {
            Debug.Assert(header.Length == tabs.Length, $"{name}: 탭 버튼과 탭 개수가 다름", this);

            for (int i = 0; i < header.Length; i++)
            {
                int index = i;
                header[i].onClick.AddListener(() => SelectTab(index));
            }
            save.onClick.AddListener(() => SaveClicked?.Invoke());
            reset.onClick.AddListener(() => ResetClicked?.Invoke());
            back.onClick.AddListener(() => BackClicked?.Invoke());
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            SelectTab(DEFAULT_TAB_INDEX);
        }

        public void Hide() => gameObject.SetActive(false);

        public void SetSaveInteractable(bool value) => save.interactable = value;

        public void RefreshTabs()
        {
            foreach (var tab in tabs) tab.Refresh();
        }

        private void SelectTab(int index)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                bool selected = i == index;
                tabs[i].gameObject.SetActive(selected);
                if (selected)
                {
                    tabs[i].Refresh();
                }
            }
        }
    }
}