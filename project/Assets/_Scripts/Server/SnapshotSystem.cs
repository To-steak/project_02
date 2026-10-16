using Project02.Enemy;
using Project02.Player;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    // 매 틱 끝. 엔티티가 많으면 여러 메시지로 나눈다. 각 메시지는 독립:
    // [int Tick][ushort 적 수][EnemySnapshot…][ushort 플레이어 수][PlayerSnapshot…]
    public static class SnapshotSystem
    {
        public const string MESSAGE = "WorldSnapshot";

        private const int MAX_MESSAGE = 1100;                                         // 비분할 전송 한도(약 1300)에서 NGO·전송 헤더 여유
        private const int HEADER_SIZE = sizeof(int) + sizeof(ushort) + sizeof(ushort);
        private const int ENEMY_SIZE = 32;                                            // 8 + 17 + 4 + 1 = 30, 여유 2
        private const int PLAYER_SIZE = 44;                                           // 8 + 17 + 4 + 4 + 8 + 1 = 42, 여유 2

        public static void Send(ServerWorld world, NetworkManager network)
        {
            if (network.ConnectedClientsIds.Count == 0) return;

            int enemyIndex = 0;
            int playerIndex = 0;

            // 엔티티가 0이어도 1번은 보낸다 (클라 보간 시계가 틱을 받아야 함)
            do
            {
                SendChunk(world, network, ref enemyIndex, ref playerIndex);
            }
            while (enemyIndex < world.Enemies.Count || playerIndex < world.Players.Count);
        }

        private static void SendChunk(ServerWorld world, NetworkManager network, ref int enemyIndex, ref int playerIndex)
        {
            int budget = MAX_MESSAGE - HEADER_SIZE;

            int enemyCount = Mathf.Min(world.Enemies.Count - enemyIndex, budget / ENEMY_SIZE);
            budget -= enemyCount * ENEMY_SIZE;
            int playerCount = Mathf.Min(world.Players.Count - playerIndex, budget / PLAYER_SIZE);

            using FastBufferWriter writer = new FastBufferWriter(MAX_MESSAGE, Allocator.Temp);
            writer.WriteValueSafe(world.Clock.Tick);

            writer.WriteValueSafe((ushort)enemyCount);
            for (int i = 0; i < enemyCount; i++)
            {
                EnemyController enemy = world.Enemies[enemyIndex + i];
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

            writer.WriteValueSafe((ushort)playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                PlayerServer player = world.Players[playerIndex + i];
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

            enemyIndex += enemyCount;
            playerIndex += playerCount;
            network.CustomMessagingManager.SendNamedMessageToAll(MESSAGE, writer, NetworkDelivery.Unreliable);
        }
    }
}