using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public enum MonsterEnum
    {
        Beholder = 0,
        BlueSlime = 1,
        Cactus = 2,
        ChestMonster = 3,
        Duo01 = 4,
        Duo02 = 5,
        Mushroom = 6,
        RedSlime = 7,
        StarFish = 8,
        TurtleShell = 9
    }

    public MonsterEnum MonsterType;

    [SerializeField] private NetworkObject[] _enemyPrefab;
    [SerializeField] private PathGrid _grid;
    [SerializeField] private Transform[] _spawnPositions;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        foreach (var point in _spawnPositions)
        {
            NetworkObject enemy = Instantiate(_enemyPrefab[(int)MonsterType], point.position, Quaternion.identity);

            enemy.GetComponent<EnemyController>().InjectGrid(_grid);
            enemy.Spawn(destroyWithScene: true);
        }
    }
}
