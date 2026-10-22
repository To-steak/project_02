using Unity.Netcode;

namespace Project02.World
{
    // Step 맨 끝: 큐에 쌓인 엔티티를 Despawn. 순회 중인 종류·묶음 목록을 수정하지 않는다
    public static class DespawnSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (NetworkObject obj in world.Despawns)
            {
                if (obj == null || !obj.IsSpawned) continue;   // Unity ==
                obj.Despawn();                                 // → OnNetworkDespawn → Unregister
            }

            world.Despawns.Clear();
        }
    }
}
