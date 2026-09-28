using System;
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

    private void Awake()
    {
        _connectButton.onClick.AddListener(OnConnect);
        _backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnConnect()
    {
        SetInteractable(false);
        _statusText.text = "connecting";

        var nm = NetworkManager.Singleton;
        nm.GetComponent<UnityTransport>().SetConnectionData(_addressInput.text.Trim(), 7777);
        nm.OnClientDisconnectCallback += OnDisconnected;
        nm.StartClient();
    }

    private void OnDisconnected(ulong clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
        _statusText.text = "can't connect to server";
        SetInteractable(true);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
    }

    private void SetInteractable(bool value)
    {
        _connectButton.interactable = value;
        _backButton.interactable = value;
        _addressInput.interactable = value;
    }

    private void OnBackClicked()
    {
        OnBack?.Invoke();
    }
}