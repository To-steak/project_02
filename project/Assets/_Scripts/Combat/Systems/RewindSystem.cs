using Project02.Enemy;
using Project02.World;

namespace Project02.Combat
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