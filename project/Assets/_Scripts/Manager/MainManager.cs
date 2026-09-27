using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject playPanel;

    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitButton;

    [SerializeField] SettingsManager settingsManager;
    [SerializeField] ConnectManager connectManager;

    void Awake()
    {
        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);

        settingsManager.OnClose += CloseSettingsPanel;
        connectManager.OnBack += ClosePlayPanel;
    }

    void OnDestroy()
    {
        settingsManager.OnClose -= CloseSettingsPanel;
        connectManager.OnBack -= ClosePlayPanel;
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
        settingsManager.OpenSettings();
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