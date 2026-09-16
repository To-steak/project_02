using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField] Button play;
    [SerializeField] Button settings;
    [SerializeField] Button exit;

    [SerializeField] SettingsManager settingsManager;
    [SerializeField] string gameSceneName = "TEST";

    void Awake()
    {
        play.onClick.AddListener(OnPlay);
        settings.onClick.AddListener(OnSettings);
        exit.onClick.AddListener(OnExit);
    }

    private void OnPlay()
    {
        play.interactable = false;
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnSettings()
    {
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
}
