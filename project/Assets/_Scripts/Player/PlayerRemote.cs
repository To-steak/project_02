using Project02.Client;
using Project02.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // Authoring (클라 World, 원격): 등록, 스냅샷 버퍼. 발사 수신은 PlayerNetwork.Fires, 표시는 RemotePlayerViewSystem
    public class PlayerRemote : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32;   // 32틱 = 640ms

        public readonly TickBuffer<PlayerSnapshot> Snapshots = new(BUFFER_SIZE);

        public PlayerNetwork Network => _network;
        public PlayerVisual Visual => _visual;
        public PlayerAnimation Animation => _animation;
        public PlayerWeapon Weapon => _weapon;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private PlayerVisual _visual;
        [SerializeField] private PlayerAnimation _animation;
        [SerializeField] private PlayerWeapon _weapon;

        public override void OnNetworkSpawn()
        {
            enabled = IsClient && !IsOwner;
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