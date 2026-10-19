using Project02.World;

namespace Project02.Enemy
{
    // 배회. 감지 범위에 플레이어가 들어오면 Battle로
    public static partial class EnemyFsmUtil
    {
        private static void EnterGeneral(EnemySimulation sim)
        {
            sim.TargetClientId = null;
        }

        private static void TickGeneral(ServerWorld world, EnemySimulation sim)
        {
            int tick = world.Clock.Tick;

            if (EnemyTargetUtil.TryFindClosest(world, sim.Movement.Position, sim.Settings.PerceptionRange, out ulong clientId))
            {
                sim.TargetClientId = clientId;
                Change(sim, EnemyStateId.Battle, tick);
                return;
            }

            sim.MoveInput = EnemyNavigationUtil.WanderInput(sim, tick);
            sim.MoveSpeed = sim.Settings.WalkSpeed;
        }
    }
}