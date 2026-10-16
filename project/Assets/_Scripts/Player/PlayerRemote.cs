using System.Collections.Generic;
using Project02.Client;
using Project02.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // Authoring + RPC 프록시: 원격 플레이어 등록, 스냅샷/발사 기록. 표시는 InterpolationSystem
    public class PlayerRemote : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32;   // 32틱 = 640ms
        public const int MAX_FIRES = 16;   // 스냅샷이 끊겨 RenderTick이 멈춰도 무한히 쌓이지 않게
        
        public PlayerController Controller => _controller;
        public readonly TickBuffer<PlayerSnapshot> Snapshots = new(BUFFER_SIZE);
        public readonly List<RemoteFire> Fires = new();

        [SerializeField] private PlayerController _controller;

        public override void OnNetworkSpawn()
        {
            enabled = IsClient && !IsOwner;
            if (!enabled) return;

            Snapshots.Clear();
            Fires.Clear();
            ClientSimulation.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled) return;

            if (ClientSimulation.Instance != null)
            {
                ClientSimulation.Instance.Unregister(this);
            }
        }

        // NGO가 아무 때나 부른다 → 기록만 (deferment)
        [Rpc(SendTo.ClientsAndHost)]
        public void FireRPC(int tick, Vector3 aimPoint, Vector2 spread)
        {
            if (!enabled) return;   // 소유자는 예측으로 이미 재생함
            Fires.Add(new RemoteFire(tick, aimPoint, spread));
        }
    }
}