using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameSettingsView : MonoBehaviour, IGameSettingsView
{
    [SerializeField] Button[] header;
    [FormerlySerializedAs("sections")]
    [SerializeField] GameSettingsTabView[] tabs;
    [SerializeField] Button save;
    [SerializeField] Button back;
    [SerializeField] Button reset;

    const int DEFAULT_TAB_INDEX = 0;

    /// <summary>뒤로가기로 화면을 닫았다. 화면 전환은 상위 View(MainMenuView)가 처리한다.</summary>
    public event Action Closed;

    GameSettingsPresenter _presenter;

    private void Awake()
    {
        Debug.Assert(header.Length == tabs.Length, $"{name}: 탭 버튼과 탭 개수가 다름", this);

        _presenter = new GameSettingsPresenter(this);

        foreach (var tab in tabs)
        {
            tab.Bind(_presenter);
        }

        for (int i = 0; i < header.Length; i++)
        {
            int index = i;
            header[i].onClick.AddListener(() => SelectTab(index));
        }
        save.onClick.AddListener(_presenter.Save);
        reset.onClick.AddListener(_presenter.Reset);
        back.onClick.AddListener(() =>
        {
            _presenter.Cancel();
            Closed?.Invoke();
        });
    }

    public void OpenSettings()
    {
        _presenter.Open();
        SelectTab(DEFAULT_TAB_INDEX);
    }

    public void SetSaveInteractable(bool value)
    {
        save.interactable = value;
    }

    public void RefreshTabs()
    {
        foreach (var tab in tabs)
        {
            tab.Refresh();
        }
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