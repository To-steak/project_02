using UnityEngine;
using UnityEngine.UI;

/// <summary>메인 메뉴의 패널 전환을 맡는 컨테이너 View.</summary>
public class MainMenuView : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject playPanel;

    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitButton;

    [SerializeField] GameSettingsView settingsView;
    [SerializeField] ConnectView connectView;

    void Awake()
    {
        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);

        settingsView.Closed += CloseSettingsPanel;
        connectView.Closed += ClosePlayPanel;
    }

    void OnDestroy()
    {
        settingsView.Closed -= CloseSettingsPanel;
        connectView.Closed -= ClosePlayPanel;
    }

    void Start()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
        playPanel.SetActive(false);
    }

    private void OnPlay()
    {
        menuPanel.SetActive(false);
        playPanel.SetActive(true);
    }

    private void OnSettings()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        settingsView.OpenSettings();
    }

    private void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void ClosePlayPanel()
    {
        playPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}