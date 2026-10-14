using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerLauncher : MonoBehaviour
{
    [SerializeField] private string _firstScene = Scenes.WaitingRoom;

    private void Start()
    {
        if (!Application.isBatchMode)
        {
            return;
        }

        if (!OpenServer())
        {
            Application.Quit(1);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Open Server")]
#endif
    public bool OpenServer()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        networkManager.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", NetworkDefaults.Port, "0.0.0.0");

        if (!networkManager.StartServer())
        {
            Debug.LogError($"[{nameof(ServerLauncher)}] failed to start on port {NetworkDefaults.Port}", this);
            return false;
        }

        Application.targetFrameRate = (int)networkManager.NetworkConfig.TickRate;
        networkManager.SceneManager.LoadScene(_firstScene, LoadSceneMode.Single);
        return true;
    }
}