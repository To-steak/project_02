using UnityEngine;
using Project02.Character;
using Project02.Weapon;

namespace Project02.Player
{
    // 유틸(쓰기): 스폰 시 1회, 이 프로세스의 사본 하나를 서버와 같은 값으로 시작한다
    // 호출처: PlayerServerAuthoring(서버 사본), PlayerOwnerAuthoring(예측 사본)의 OnNetworkPostSpawn
    public static class PlayerSpawnUtil
    {
        public static void Initialize(PlayerSimulation sim, Vector3 position, SOMovement movement, SOCatalog catalog, int equippedId, in SOLevelTable.Row stats)
        {
            sim.Movement = movement;
            sim.Catalog = catalog;

            sim.State = default;
            sim.State.Movement = new MovementState { Position = position };

            PlayerEquipUtil.Apply(sim, equippedId);
            PlayerStatUtil.ApplyStamina(sim, stats);
        }
    }
}