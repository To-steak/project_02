using Project02.Enemy;

namespace Project02.Server
{
    public static class RewindSystem
    {
        public static void Record(ServerWorld world)
        {
            int tick = world.Clock.Tick;
            foreach (EnemyServerAuthoring enemy in world.Enemies)
            {
                enemy.Rewind.History.SetValue(tick, enemy.Simulation.Movement.Position);
            }
        }
    }
}