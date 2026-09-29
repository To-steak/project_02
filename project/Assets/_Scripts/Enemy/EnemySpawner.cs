using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject _enemyPrefab;
    [SerializeField] private PathGrid _grid;
    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private bool _spawnOnStart = true;

    public override void OnNetworkSpawn()
    {
        if (!IsServer || !_spawnOnStart) return;

        // 서버가 먼저 씬에 들어와도, 클라이언트가 다 들어온 뒤에 스폰한다.
        // 먼저 스폰하면 로딩 중인 클라이언트가 이전 씬(대기실)에 적을 만들었다가 씬 전환 때 잃어버린다.
        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.SceneManager != null)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> completed, List<ulong> timedOut)
    {
        if (sceneName != gameObject.scene.name) return;

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        SpawnEnemy();
    }

    [ContextMenu("Spawn Enemy")]
    private void SpawnEnemy()
    {
        if (!IsServer)
        {
            Debug.LogWarning($"[{nameof(EnemySpawner)}] 서버에서만 소환할 수 있다.", this);
            return;
        }

        NetworkObject enemy = Instantiate(_enemyPrefab, _spawnPosition.position, Quaternion.identity);
        enemy.GetComponent<EnemyController>().InjectGrid(_grid);
        enemy.Spawn(destroyWithScene: true);
    }
}