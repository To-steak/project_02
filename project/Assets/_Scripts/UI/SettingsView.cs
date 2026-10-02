using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : MonoBehaviour, ISettingsView
{
    [SerializeField] Button[] header;
    [SerializeField] SettingsSection[] sections;
    [SerializeField] Button save;
    [SerializeField] Button back;
    [SerializeField] Button reset;

    const int DEFAULT_SECTION_INDEX = 0;

    public event Action SaveRequested;
    public event Action BackRequested;
    public event Action ResetRequested;

    public event Action OnClose; // MainMenuView가 구독하는 닫힘 이벤트.

    SettingsPresenter _presenter;

    private void Awake()
    {
        Debug.Assert(header.Length == sections.Length, $"{name}: 탭 버튼과 섹션 개수가 다름", this);

        _presenter = new SettingsPresenter(this, GameServices.Settings);
        _presenter.Closed += () => OnClose?.Invoke();

        foreach (var section in sections)
        {
            section.Bind(_presenter);
        }

        for (int i = 0; i < header.Length; i++)
        {
            int index = i;
            header[i].onClick.AddListener(() => SelectSection(index));
        }
        save.onClick.AddListener(() => SaveRequested?.Invoke());
        back.onClick.AddListener(() => BackRequested?.Invoke());
        reset.onClick.AddListener(() => ResetRequested?.Invoke());
    }

    private void OnDestroy()
    {
        _presenter?.Dispose();
    }

    public void OpenSettings()
    {
        _presenter.Open();
        SelectSection(DEFAULT_SECTION_INDEX);
    }

    public void SetSaveBtnInteractable(bool value)
    {
        save.interactable = value;
    }

    public void RefreshSections()
    {
        foreach (var section in sections)
        {
            section.Refresh();
        }
    }

    private void SelectSection(int index)
    {
        for (int i = 0; i < sections.Length; i++)
        {
            bool selected = i == index;
            sections[i].gameObject.SetActive(selected);
            if (selected)
            {
                sections[i].Refresh();
            }
        }
    }
}