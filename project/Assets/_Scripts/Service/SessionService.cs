using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 접속 승인과 현재 접속 중인 클라이언트(clientId → userId)를 관리한다.
/// 진행 데이터는 ProgressService에 userId로 요청한다.
/// </summary>
public class SessionService : MonoBehaviour
{
    public static SessionService Instance { get; private set; }

    public event Action<ulong, int> ExpChanged;

    private NetworkManager _networkManager;
    private readonly Dictionary<ulong, string> _userIds = new();

    private const int MAX_PAYLOAD = 256;

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();
        _networkManager.ConnectionApprovalCallback = Approve;
        _networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public string GetNickname(ulong clientId)
    {
        return _userIds.TryGetValue(clientId, out string userId) ? ProgressService.Instance.GetNickname(userId) : null;
    }

    public int GetExp(ulong clientId)
    {
        return _userIds.TryGetValue(clientId, out string userId) ? ProgressService.Instance.GetExp(userId) : 0;
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

        if (ProgressService.Instance.Exists(payload.UserId))
        {
            Accept(request, response, payload.UserId);
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

        if (ProgressService.Instance.IsNicknameTaken(nickname))
        {
            Reject(response, ConnectReject.NicknameTaken);
            return;
        }

        ProgressService.Instance.Register(payload.UserId, nickname);
        Accept(request, response, payload.UserId);
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
        foreach (var pair in _userIds)
        {
            if (pair.Value != userId)
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
            _userIds.Remove(stale.Value);
        }

        return false;
    }

    private void Accept(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response, string userId)
    {
        _userIds[request.ClientNetworkId] = userId;
        response.Approved = true;
    }

    private static void Reject(NetworkManager.ConnectionApprovalResponse response, ConnectReject reason)
    {
        response.Approved = false;
        response.Reason = reason.ToString();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        _userIds.Remove(clientId);
    }

    public void AddReward(IReadOnlyList<ulong> clientIds, int exp, int gold)
    {
        if (_networkManager == null || !_networkManager.IsServer || (exp <= 0 && gold <= 0))
        {
            return;
        }

        var rewarded = new List<(ulong clientId, string userId)>();
        foreach (ulong clientId in clientIds)
        {
            if (!_userIds.TryGetValue(clientId, out string userId) || !ProgressService.Instance.Exists(userId))
            {
                Debug.LogError($"{nameof(SessionService)} no profile for client {clientId}");
                continue;
            }

            rewarded.Add((clientId, userId));
        }

        if (rewarded.Count == 0)
        {
            return;
        }

        var userIds = new List<string>(rewarded.Count);
        foreach (var (_, userId) in rewarded)
        {
            userIds.Add(userId);
        }
        ProgressService.Instance.AddReward(userIds, exp, gold);

        foreach (var (clientId, userId) in rewarded)
        {
            ExpChanged?.Invoke(clientId, ProgressService.Instance.GetExp(userId));
        }
    }
}