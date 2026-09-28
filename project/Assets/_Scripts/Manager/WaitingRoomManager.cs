using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomManager : NetworkBehaviour
{
    [SerializeField] private Button _tutorialWorld;
    [SerializeField] private Button _testWorld;

    private const int TUTORIAL_WORLD_INDEX = 0;
    private const int TEST_WORLD_INDEX = 1;

    private void Awake()
    {
        _tutorialWorld.onClick.AddListener(OnTutorialClicked);
        _testWorld.onClick.AddListener(OnTestWorldClicked);
    }

    private void OnTutorialClicked()
    {
        RequestWorldSelectionRPC(TUTORIAL_WORLD_INDEX);
    }

    private void OnTestWorldClicked()
    {
        RequestWorldSelectionRPC(TEST_WORLD_INDEX);
    }

    [Rpc(SendTo.Server)]
    private void RequestWorldSelectionRPC(int index)
    {
        switch (index)
        {
            case TUTORIAL_WORLD_INDEX:
                Debug.Log("Tutorial World Selected");
                NetworkManager.SceneManager.LoadScene("TUTORIAL WORLD", UnityEngine.SceneManagement.LoadSceneMode.Single);
                break;
            case TEST_WORLD_INDEX:
                Debug.Log("Test World Selected");
                NetworkManager.SceneManager.LoadScene("TEST WORLD", UnityEngine.SceneManagement.LoadSceneMode.Single);
                break;
            default:
                Debug.Log($"{index} is not valid");
                break;
        }
    }
}