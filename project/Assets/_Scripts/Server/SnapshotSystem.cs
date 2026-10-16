using Project02.Enemy;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    public static class SnapshotSystem
    {
        public const string MESSAGE = "EnemySnapshot";
        private const int SNAPSHOT_SIZE = 32;   // 8 + 12 + 4 + 1 + 4 + 1 = 30, 여유 2

        public static void Send(ServerWorld world, NetworkManager network)
        {
            if (network.ConnectedClientsIds.Count == 0) return;

            int size = sizeof(int) + sizeof(ushort) + world.Enemies.Count * SNAPSHOT_SIZE;
            Debug.Assert(size < 1200, "warning : a lots of enemy!");

            using FastBufferWriter writer = new FastBufferWriter(size, Allocator.Temp);
            writer.WriteValueSafe(world.Clock.Tick);
            writer.WriteValueSafe((ushort)world.Enemies.Count);
            foreach (EnemyController enemy in world.Enemies)
            {
                writer.WriteNetworkSerializable(enemy.Capture());
            }

            network.CustomMessagingManager.SendNamedMessageToAll(MESSAGE, writer, NetworkDelivery.Unreliable);
        }
    }
}