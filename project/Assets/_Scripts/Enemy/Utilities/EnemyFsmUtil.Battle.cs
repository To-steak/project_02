using UnityEngine;
using Project02.World;

namespace Project02.Enemy
{
    // 타깃 추적. 공격 범위 밖이면 달려가고, 전투 범위를 벗어나면 General로
    public static partial class EnemyFsmUtil
    {
        private static void TickBattle(ServerWorld world, EnemySimulation sim)
        {
            SOEnemy settings = sim.Settings;

            if (!EnemyTargetUtil.TryGetTarget(world, sim, out Vector3 target))
            {
                if (EnemyTargetUtil.TryFindClosest(world, sim.Movement.Position, settings.PerceptionRange, out ulong next))
                {
                    sim.TargetClientId = next;
                    return;
                }

                Change(sim, EnemyStateId.General, world.Clock.Tick);
                return;
            }

            Vector3 direction = target - sim.Movement.Position;
            direction.y = 0.0f;
            float distance = direction.magnitude;
            if (distance > settings.BattleRange)
            {
                Change(sim, EnemyStateId.General, world.Clock.Tick);
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