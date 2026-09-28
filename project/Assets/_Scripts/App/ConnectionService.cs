using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionService : MonoBehaviour, IConnectionService
{
    public event Action<ConnectState> StateChanged;

    private NetworkManager _networkManager;

    private void Awake()
    {
        if (GameServices.Connection != null)
        {
            Destroy(this);
            return;
        }

        _networkManager = GetComponent<NetworkManager>();
        GameServices.Register(this);
    }

    private void OnDestroy()
    {
        Unsubscribe();
        GameServices.Unregister(this);
    }

    private void OnConnected(ulong id)
    {
        Unsubscribe();
        StateChanged?.Invoke(ConnectState.Connected);
    }

    private void OnDisconnected(ulong id)
    {
        ConnectionFail();
    }

    private void Unsubscribe()
    {
        if (_networkManager != null)
        {
            _networkManager.OnClientConnectedCallback -= OnConnected;
            _networkManager.OnClientDisconnectCallback -= OnDisconnected;
        }
    }

    private void ConnectionFail()
    {
        Unsubscribe();
        StateChanged?.Invoke(ConnectState.Failed);
    }

    public void Connect(string address)
    {
        if (_networkManager.IsClient)
        {
            return;
        }
        
        StateChanged?.Invoke(ConnectState.Connecting);

        _networkManager.GetComponent<UnityTransport>().SetConnectionData(address, NetworkDefaults.Port);
        _networkManager.OnClientConnectedCallback += OnConnected;
        _networkManager.OnClientDisconnectCallback += OnDisconnected;

        if (!_networkManager.StartClient())
        {
            ConnectionFail();
        }
    }
}
