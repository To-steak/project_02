using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private void Start()
    {
        if (!Application.isBatchMode) return;

        NetworkManager nm = NetworkManager.Singleton;
        nm.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
        nm.StartServer();
        nm.SceneManager.LoadScene("WAITING ROOM", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}