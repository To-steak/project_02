using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Transform[] _spawnPoints;

    private int _nextSpawnIndex;

    void Start()
    {
        Application.targetFrameRate = 144;

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

#if UNITY_SERVER
        NetworkManager.Singleton.StartServer();
#endif
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
                               NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;

        GetSpawnPose(out var position, out var rotation);
        response.Position = position;
        response.Rotation = rotation;

        Debug.Log($"[Approval] client:{request.ClientNetworkId} spawn:{position}");
    }

    private void GetSpawnPose(out Vector3 position, out Quaternion rotation)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }

        var point = _spawnPoints[_nextSpawnIndex % _spawnPoints.Length];
        _nextSpawnIndex++;

        position = point.position;
        rotation = point.rotation;
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
}