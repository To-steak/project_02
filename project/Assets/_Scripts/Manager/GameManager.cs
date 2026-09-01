using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CinemachineCamera cinemachine;

    void Start()
    {
        Application.targetFrameRate = 144;
        
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

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void CinemachineTarget()
    {
        
    }
}
