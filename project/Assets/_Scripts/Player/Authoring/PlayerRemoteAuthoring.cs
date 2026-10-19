using Project02.Client;
using Project02.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // Authoring (클라 World, 원격): 등록, 스냅샷 버퍼. 발사 수신은 PlayerNetwork.Fires, 표시는 RemotePlayerViewSystem
    public class PlayerRemoteAuthoring : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32;   // 32틱 = 640ms

        public readonly TickBuffer<PlayerSnapshot> Snapshots = new(BUFFER_SIZE);

        public PlayerNetwork Network => _network;
        public PlayerBodyView Visual => _visual;
        public PlayerAnimatorView Animation => _animation;
        public PlayerWeaponView Weapon => _weapon;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private PlayerBodyView _visual;
        [SerializeField] private PlayerAnimatorView _animation;
        [SerializeField] private PlayerWeaponView _weapon;

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