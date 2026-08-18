using Unity.Netcode;
using UnityEngine;

public class ConnectionLogger : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnServerStarted()
    {
        Debug.Log("=== SERVER STARTED ===");
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"=== CLIENT CONNECTED: {clientId} ===");
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"=== CLIENT DISCONNECTED: {clientId} ===");
    }
}