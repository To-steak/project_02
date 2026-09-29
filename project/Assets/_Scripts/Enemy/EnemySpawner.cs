using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject _enemyPrefab;
    [SerializeField] private PathGrid _grid;
    [SerializeField] private Transform _spawnPosition;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        NetworkObject enemy = Instantiate(_enemyPrefab, _spawnPosition.position, Quaternion.identity);
        enemy.GetComponent<EnemyController>().InjectGrid(_grid);
        enemy.Spawn(destroyWithScene: true);
    }
}