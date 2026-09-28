using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private int _nextSpawnIndex;

    private void Start()
    {
        var nm = NetworkManager.Singleton;
        if (!nm.IsServer) return;

        // 이미 연결된 클라이언트(호스트 자신 포함)
        foreach (ulong clientId in nm.ConnectedClientsIds)
            SpawnPlayer(clientId);

        // 이후 접속해서 씬 동기화를 마친 클라이언트
        nm.SceneManager.OnSynchronizeComplete += SpawnPlayer;
    }

    private void OnDestroy()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.SceneManager != null)
            nm.SceneManager.OnSynchronizeComplete -= SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (!nm.ConnectedClients.TryGetValue(clientId, out var client) || client.PlayerObject != null)
            return;

        GetSpawnPose(out var position, out var rotation);
        NetworkObject player = Instantiate(_playerPrefab, position, rotation);
        player.SpawnAsPlayerObject(clientId);
    }

    private void GetSpawnPose(out Vector3 position, out Quaternion rotation)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }

        var point = _spawnPoints[_nextSpawnIndex++ % _spawnPoints.Length];
        position = point.position;
        rotation = point.rotation;
    }
}