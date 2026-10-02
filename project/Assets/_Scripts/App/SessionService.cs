using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class SessionService : MonoBehaviour, ISessionService
{
    private const int MAX_PAYLOAD = 256;
    private const string PROFILE_FILE = "profiles.json";

    private NetworkManager _networkManager;
    private IProfileStore _store;
    private readonly Dictionary<ulong, PlayerProfile> _session = new();
    private readonly Dictionary<ulong, string> _userIds = new();

    private IProfileStore Store => _store ??= new JsonProfileStore(Path.Combine(Application.persistentDataPath, PROFILE_FILE));

    private void Awake()
    {
        if (GameServices.Session != null)
        {
            Destroy(this);
            return;
        }

        _networkManager = GetComponent<NetworkManager>();
        _networkManager.ConnectionApprovalCallback = Approve;
        _networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        GameServices.Register(this);
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.ConnectionApprovalCallback = null;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        GameServices.Unregister(this);
    }

    public string GetNickname(ulong clientId)
    {
        return _session.TryGetValue(clientId, out var profile) ? profile.Nickname : null;
    }

    private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.CreatePlayerObject = false;
        response.Pending = false;

        if (!TryReadPayload(request.Payload, out ConnectPayload payload))
        {
            Reject(response, ConnectReject.InvalidPayload);
            return;
        }

        if (IsUserConnected(payload.UserId))
        {
            Reject(response, ConnectReject.AlreadyConnected);
            return;
        }

        if (Store.TryGet(payload.UserId, out PlayerProfile profile))
        {
            Accept(request, response, profile);
            return;
        }

        if (string.IsNullOrEmpty(payload.Nickname))
        {
            Reject(response, ConnectReject.NicknameRequired);
            return;
        }

        if (!NicknameRule.TryNormalize(payload.Nickname, out string nickname, out _))
        {
            Reject(response, ConnectReject.NicknameInvalid);
            return;
        }

        if (Store.IsNicknameTaken(nickname))
        {
            Reject(response, ConnectReject.NicknameTaken);
            return;
        }

        profile = new PlayerProfile
        {
            UserId = payload.UserId,
            Nickname = nickname
        };
        Store.Add(profile);
        Accept(request, response, profile);
    }

    private static bool TryReadPayload(byte[] bytes, out ConnectPayload payload)
    {
        payload = null;
        if (bytes == null || bytes.Length == 0 || bytes.Length > MAX_PAYLOAD)
        {
            return false;
        }

        try
        {
            payload = JsonUtility.FromJson<ConnectPayload>(Encoding.UTF8.GetString(bytes));
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (payload == null || !Guid.TryParse(payload.UserId, out var guid))
        {
            return false;
        }

        payload.UserId = guid.ToString("N");
        return true;
    }

    private bool IsUserConnected(string userId)
    {
        ulong? stale = null;
        foreach (var pair in _session)
        {
            if (pair.Value.UserId != userId)
            {
                continue;
            }

            if (_networkManager.ConnectedClients.ContainsKey(pair.Key))
            {
                return true;
            }

            stale = pair.Key;
            break;
        }

        if (stale.HasValue)
        {
            _session.Remove(stale.Value);
        }

        return false;
    }

    private void Accept(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response, PlayerProfile profile)
    {
        _session[request.ClientNetworkId] = profile;
        _userIds[request.ClientNetworkId] = profile.UserId;
        response.Approved = true;
    }

    private static void Reject(NetworkManager.ConnectionApprovalResponse response, ConnectReject reason)
    {
        response.Approved = false;
        response.Reason = reason.ToString();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        _session.Remove(clientId);
    }

    public void AddReward(IReadOnlyList<ulong> clientIds, int exp, int gold)
    {
        if (_networkManager == null || !_networkManager.IsServer || (exp <= 0 && gold <= 0))
        {
            return;
        }

        bool changed = false;
        foreach (ulong clientId in clientIds)
        {
            if (!_userIds.TryGetValue(clientId, out string userId) || !Store.TryGet(userId, out var profile))
            {
                Debug.LogError($"{nameof(SessionService)} no profile for client {clientId}");
                continue;
            }

            profile.Exp += exp;
            profile.Gold += gold;

            changed = true;
        }

        if (changed)
        {
            Store.Save();
        }
    }
}