using System;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _addressInput;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _statusText;

    public event Action OnBack;

    private const ushort DEFAULT_PORT = 7777;
    private const string PREF_KEY = "LastAddress";

    private void Awake()
    {
        _connectButton.onClick.AddListener(OnConnect);
        _backButton.onClick.AddListener(() => OnBack?.Invoke());
    }

    private void OnEnable()
    {
        _addressInput.text = PlayerPrefs.GetString(PREF_KEY, "127.0.0.1");
        SetStatus("");
        SetInteractable(true);
    }

    private async void OnConnect()
    {
        SetInteractable(false);
        SetStatus("접속 중...");

        if (!TryParseEndpoint(_addressInput.text, out string host, out ushort port))
        {
            Fail("주소 형식이 올바르지 않습니다. 예: 192.168.0.10:7777");
            return;
        }

        string ip = await ResolveAsync(host);
        if (ip == null)
        {
            Fail($"'{host}'를 찾을 수 없습니다.");
            return;
        }

        PlayerPrefs.SetString(PREF_KEY, _addressInput.text.Trim());

        var nm = NetworkManager.Singleton;
        nm.GetComponent<UnityTransport>().SetConnectionData(ip, port);
        nm.OnClientDisconnectCallback += OnDisconnected;

        if (!nm.StartClient())
        {
            Fail("클라이언트를 시작하지 못했습니다.");
        }
    }

    private void OnDisconnected(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (clientId != nm.LocalClientId) return;

        nm.OnClientDisconnectCallback -= OnDisconnected;
        string reason = nm.DisconnectReason;
        Fail(string.IsNullOrEmpty(reason) ? "서버에 연결할 수 없습니다." : reason);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
    }

    // "host", "host:port" 둘 다 허용
    private static bool TryParseEndpoint(string input, out string host, out ushort port)
    {
        host = null;
        port = DEFAULT_PORT;
        input = input?.Trim();
        if (string.IsNullOrEmpty(input)) return false;

        int colon = input.LastIndexOf(':');
        if (colon >= 0)
        {
            if (!ushort.TryParse(input[(colon + 1)..], out port) || port == 0) return false;
            input = input[..colon];
        }

        host = input;
        return host.Length > 0;
    }

    private static async Task<string> ResolveAsync(string host)
    {
        if (IPAddress.TryParse(host, out _)) return host;
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            foreach (var a in addresses)
                if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return a.ToString();
        }
        catch { }
        return null;
    }

    private void Fail(string message)
    {
        SetStatus(message);
        SetInteractable(true);
    }

    private void SetStatus(string message)
    {
        if (_statusText != null) _statusText.text = message;
    }

    private void SetInteractable(bool value)
    {
        _connectButton.interactable = value;
        _backButton.interactable = value;
        _addressInput.interactable = value;
    }
}