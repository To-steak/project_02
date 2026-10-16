using Project02.Client;
using Project02.Core;
using Unity.Netcode;

namespace Project02.Enemy
{
    // Authoring: 클라 World 등록 + 스냅샷 버퍼. 표시는 EnemyViewSystem
    public class EnemyRemote : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32;
        public readonly TickBuffer<EnemySnapshot> Snapshots = new(BUFFER_SIZE);

        public override void OnNetworkSpawn()
        {
            // 클라 World가 있는 프로세스면 등록 (순수 클라 + 호스트). !IsServer로 판정하지 않는다
            enabled = IsClient;
            if (!enabled) return;

            Snapshots.Clear();
            ClientLoop.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled) return;

            if (ClientLoop.Instance != null)
            {
                ClientLoop.Instance.Unregister(this);
            }
        }
    }
}