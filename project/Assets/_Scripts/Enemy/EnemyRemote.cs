using Project02.Character;
using Project02.Client;
using Unity.Netcode;

namespace Project02.Enemy
{
    public class EnemyRemote : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32; // 32 tick = 640ms
        public const int BUFFER_MASK = BUFFER_SIZE - 1;

        public readonly EnemySnapshot[] Snapshots = new EnemySnapshot[BUFFER_SIZE];
        public readonly int[] Ticks = new int[BUFFER_SIZE];

        public EnemyStateId ViewStateId;
        public MovementState ViewMovement;

        public override void OnNetworkSpawn()
        {
            enabled = !IsServer;
            if (!enabled)
            {
                return;
            }

            for (int i = 0; i < Ticks.Length; i++)
            {
                Ticks[i] = -1;
            }

            ClientSimulation.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled)
            {
                return;
            }

            ClientSimulation.Instance.Unregister(this);
        }

        public void Record(int tick, in EnemySnapshot snapshot)
        {
            Snapshots[tick & BUFFER_MASK] = snapshot;
            Ticks[tick & BUFFER_MASK] = tick;
        }
    }
}