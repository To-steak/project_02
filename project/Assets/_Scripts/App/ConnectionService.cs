using System;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionService : MonoBehaviour, IConnectionService
{
    public event Action<ConnectState> StateChanged;

    private NetworkManager _networkManager;
    private const string USER_ID_KEY = "UserId";
    public ConnectReject FailReason { get; private set; }

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
        _networkManager.OnClientConnectedCallback -= OnConnected;
        StateChanged?.Invoke(ConnectState.Connected);
    }

    private void OnDisconnected(ulong id)
    {
        Unsubscribe();

        bool isConnected = _networkManager.IsConnectedClient;
        if (isConnected)
        {
            StateChanged?.Invoke(ConnectState.Disconnected);
            SceneManager.LoadScene("MAIN MENU");
        }
        else
        {
            FailReason = ParseReason(_networkManager.DisconnectReason);
            StateChanged?.Invoke(ConnectState.Failed);
        }
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

    public void Connect(string address, string nickname)
    {
        if (_networkManager.IsClient)
        {
            return;
        }

        FailReason = ConnectReject.None;
        StateChanged?.Invoke(ConnectState.Connecting);

        var payload = new ConnectPayload
        {
            UserId = CreateUserId(),
            Nickname = nickname
        };
        _networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

        _networkManager.GetComponent<UnityTransport>().SetConnectionData(address, NetworkDefaults.Port);
        _networkManager.OnClientConnectedCallback += OnConnected;
        _networkManager.OnClientDisconnectCallback += OnDisconnected;

        if (!_networkManager.StartClient())
        {
            ConnectionFail();
        }
    }

    public void Disconnect()
    {
        Unsubscribe();

        _networkManager.Shutdown();
        SceneManager.LoadScene("MAIN MENU");
    }

    private static string CreateUserId()
    {
        string key = GetUserIdKey();
        string id = PlayerPrefs.GetString(key, string.Empty);
        if (Guid.TryParse(id, out _))
        {
            return id;
        }

        id = Guid.NewGuid().ToString("N");
        PlayerPrefs.SetString(key, id);
        PlayerPrefs.Save();
        return id;
    }

    private static string GetUserIdKey()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-profile")
            {
                return $"{USER_ID_KEY}_{args[i + 1]}";
            }
        }

#if UNITY_EDITOR
        return $"{USER_ID_KEY}_{Hash128.Compute(Application.dataPath)}";
#else
    return USER_ID_KEY;
# endif
    }

    private static ConnectReject ParseReason(string reason)
    {
        if (Enum.TryParse(reason, out ConnectReject result) && Enum.IsDefined(typeof(ConnectReject), result))
        {
            return result;
        }

        return ConnectReject.None;
    }

    [ContextMenu("Connect Local Client")]
    private void ConnectClient() => GameServices.Connection.Connect("127.0.0.1", null);
}
