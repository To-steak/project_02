using Project02.Combat;
using Project02.Enemy;

namespace Project02.Server
{
    public static class DeathSystem
    {
        public static void Process(ServerWorld world)
        {
            // 뒤에서부터: Despawn → OnNetworkDespawn → Unregister가 이 목록에서 자신을 지운다
            for (int i = world.Enemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = world.Enemies[i];
                Health health = enemy.Health;
                if (!health.IsDead) continue;

                int count = health.Contributors.Count;
                if (count > 0)
                {
                    int exp = enemy.Settings.RewardExp / count;
                    int gold = enemy.Settings.RewardGold / count;

                    // 복사: Despawn 뒤 RewardSystem이 읽을 때 원본이 비워져 있어도 안전하게
                    world.Rewards.Add(new RewardInfo(health.Contributors.ToArray(), exp, gold));
                }

                enemy.NetworkObject.Despawn();
            }
        }
    }
}