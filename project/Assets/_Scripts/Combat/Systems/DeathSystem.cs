using Project02.Enemy;
using Project02.World;

namespace Project02.Combat
{
    // 죽은 적을 찾아(질의) 보상과 Despawn을 큐에 넣는다. 실제 Despawn은 DespawnSystem (Step 끝)
    public static class DeathSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (EnemyServerAuthoring enemy in world.Enemies)
            {
                Health health = enemy.Health;
                if (!health.IsDead) continue;

                int count = health.Contributors.Count;
                if (count > 0)
                {
                    int exp = enemy.Simulation.Settings.RewardExp / count;
                    int gold = enemy.Simulation.Settings.RewardGold / count;

                    // 복사: SessionSystem이 처리하기 전에 원본이 바뀌어도 안전하게
                    world.Rewards.Add(new RewardInfo(health.Contributors.ToArray(), exp, gold));
                }

                world.Despawns.Add(enemy);
            }
        }
    }
}