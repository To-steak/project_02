using Project02.World;

namespace Project02.Enemy
{
    public static class GeneralState
    {
        public static void Enter(EnemySimulation sim)
        {
            sim.TargetClientId = null;
        }

        public static void Tick(ServerWorld world, EnemySimulation sim, float deltaTime)
        {
            int tick = world.Clock.Tick;

            if (EnemyTargetUtil.TryFindClosest(world, sim.Movement.Position, sim.Settings.PerceptionRange, out ulong clientId))
            {
                sim.TargetClientId = clientId;
                EnemyFsmUtil.Change(sim, EnemyStateId.Battle, tick);
                return;
            }

            sim.MoveInput = EnemyNavigationUtil.WanderInput(sim, tick);
            sim.MoveSpeed = sim.Settings.WalkSpeed;
        }
    }
}