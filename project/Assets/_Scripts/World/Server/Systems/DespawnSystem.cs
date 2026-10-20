namespace Project02.World
{
    // Step 맨 끝: 큐에 쌓인 엔티티를 Despawn. 순회 중인 world.Enemies를 수정하지 않는다
    public static class DespawnSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (var enemy in world.Despawns)
            {
                if (enemy == null || !enemy.IsSpawned) continue;   // Unity ==
                enemy.NetworkObject.Despawn();                     // → OnNetworkDespawn → Unregister
            }

            world.Despawns.Clear();
        }
    }
}