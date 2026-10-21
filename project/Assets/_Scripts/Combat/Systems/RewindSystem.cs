using Project02.Enemy;
using Project02.Player;
using Project02.World;

namespace Project02.Combat
{
    // 기록은 정본을 가진 쪽, 판정은 묶음 목록.
    // 위치의 정본(MovementState.Position)은 종류마다 다른 곳에 있으므로 기록은 종류 목록을 순회한다.
    // 판정(ProjectileSystem)은 world.Hittables만 본다 (규칙 11)
    public static class RewindSystem
    {
        public static void Record(ServerWorld world)
        {
            int tick = world.Clock.Tick;

            foreach (EnemyServerAuthoring enemy in world.Enemies)
            {
                enemy.Rewind.History.SetValue(tick, enemy.Simulation.Movement.Position);
            }

            foreach (PlayerServerAuthoring player in world.Players)
            {
                player.Rewind.History.SetValue(tick, player.Simulation.State.Movement.Position);
            }
        }
    }
}
