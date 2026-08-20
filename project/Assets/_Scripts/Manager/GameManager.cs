using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
#if UNITY_SERVER
        NetworkManager.Singleton.StartServer();
#endif
    }

    public void ConnectClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void OpenServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}
