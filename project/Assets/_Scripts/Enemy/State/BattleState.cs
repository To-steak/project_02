using Project02.Server;
using UnityEngine;

namespace Project02.Enemy
{
    // 타깃 추적. 공격 범위 밖이면 달려가고, 전투 범위를 벗어나면 배회로
    public static class BattleState
    {
        public static void Tick(ServerWorld world, EnemySimulation sim, float deltaTime)
        {
            SOEnemy settings = sim.Settings;

            if (!EnemyTargetUtil.TryGetTarget(world, sim, out Vector3 target))
            {
                if (EnemyTargetUtil.TryFindClosest(world, sim.Movement.Position, settings.PerceptionRange, out ulong next))
                {
                    sim.TargetClientId = next;
                    return;
                }

                EnemyFsm.Change(sim, EnemyStateId.General);
                return;
            }

            Vector3 direction = target - sim.Movement.Position;
            direction.y = 0.0f;
            float distance = direction.magnitude;
            if (distance > settings.BattleRange)
            {
                EnemyFsm.Change(sim, EnemyStateId.General);
                return;
            }

            if (distance > settings.AttackRange)
            {
                sim.MoveInput = new Vector2(direction.x, direction.z).normalized;
                sim.MoveSpeed = settings.RunSpeed;
            }
        }
    }
}