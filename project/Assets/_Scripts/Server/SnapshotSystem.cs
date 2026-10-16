using Project02.Enemy;
using Project02.Player;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    // 매 틱 끝: [int Tick][ushort 적 수][EnemySnapshot…][ushort 플레이어 수][PlayerSnapshot…]
    public static class SnapshotSystem
    {
        public const string MESSAGE = "WorldSnapshot";
        private const int ENEMY_SIZE = 32;    // 8 + 17 + 4 + 1 = 30, 여유 2
        private const int PLAYER_SIZE = 44;   // 8 + 17 + 4 + 4 + 8 + 1 = 42, 여유 2

        public static void Send(ServerWorld world, NetworkManager network)
        {
            if (network.ConnectedClientsIds.Count == 0) return;

            int size = sizeof(int) + sizeof(ushort) * 2 + world.Enemies.Count * ENEMY_SIZE + world.Players.Count * PLAYER_SIZE;
            Debug.Assert(size < 1200, "warning : snapshot exceeds MTU");

            using FastBufferWriter writer = new FastBufferWriter(size, Allocator.Temp);
            writer.WriteValueSafe(world.Clock.Tick);

            writer.WriteValueSafe((ushort)world.Enemies.Count);
            foreach (EnemyController enemy in world.Enemies)
            {
                EnemySimulation sim = enemy.Simulation;
                EnemySnapshot snapshot = new EnemySnapshot
                {
                    ObjectId = enemy.NetworkObjectId,
                    Movement = sim.Movement,
                    Yaw = sim.Rotation.eulerAngles.y,
                    StateId = sim.StateId,
                };
                writer.WriteNetworkSerializable(snapshot);
            }

            writer.WriteValueSafe((ushort)world.Players.Count);
            foreach (PlayerServer player in world.Players)
            {
                PlayerSimulation sim = player.Controller.Simulation;
                PlayerSnapshot snapshot = new PlayerSnapshot
                {
                    ObjectId = player.NetworkObjectId,
                    Movement = sim.State.Movement,
                    Yaw = sim.Yaw,
                    Pitch = sim.Pitch,
                    Move = sim.Move,
                    IsRunning = sim.IsRunning,
                };
                writer.WriteNetworkSerializable(snapshot);
            }

            network.CustomMessagingManager.SendNamedMessageToAll(MESSAGE, writer, NetworkDelivery.Unreliable);
        }
    }
}