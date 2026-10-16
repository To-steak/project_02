using Project02.Server;

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
            if (EnemyTargetUtil.TryFindClosest(world, sim.Movement.Position, sim.Settings.PerceptionRange, out ulong clientId))
            {
                sim.TargetClientId = clientId;
                EnemyFsm.Change(sim, EnemyStateId.Battle);
                return;
            }

            sim.MoveInput = EnemyNavigation.WanderInput(sim, deltaTime);
            sim.MoveSpeed = sim.Settings.WalkSpeed;
        }
    }
}