using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject settingsPanel;

    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitButton;

    [SerializeField] SettingsManager settingsManager;
    const string SCENE_NAME = "TEST";

    void Awake()
    {
        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);

        settingsManager.OnClose += CloseSettingsPanel;
    }

    void OnDestroy()
    {
        settingsManager.OnClose -= CloseSettingsPanel;
    }

    void Start()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void OnPlay()
    {
        playButton.interactable = false;
        SceneManager.LoadScene(SCENE_NAME);
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
}
