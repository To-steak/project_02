using Project02.Character;
using Project02.Client;
using Project02.Core;
using Unity.Netcode;

namespace Project02.Enemy
{
    public class EnemyRemote : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32;
        public readonly TickBuffer<EnemySnapshot> Snapshots = new(BUFFER_SIZE);

        public override void OnNetworkSpawn()
        {
            enabled = !IsServer;
            if (!enabled)
            {
                return;
            }

            Snapshots.Clear();
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
    }
}