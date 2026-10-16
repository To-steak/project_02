using Project02.Server;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // Authoring + RPC 프록시: 서버 World 등록, 서버 사본 소유, 받은 입력을 쌓기만 한다
    public class PlayerServer : NetworkBehaviour
    {
        public PlayerController Controller => _controller;
        public readonly PlayerSimulation Simulation = new();   // 서버 권한 상태
        public readonly InputQueue Inputs = new();

        [SerializeField] private PlayerController _controller;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            ServerLoop.Instance.Register(this);   // SessionSystem.Join → Nickname, Level
        }

        protected override void OnNetworkPostSpawn()
        {
            if (!IsServer) return;

            // 스폰 시 1회 (Authoring). Join이 넣은 Level, OnNetworkSpawn의 EquippedWeapon이 이미 있다
            PlayerSpawnUtil.Initialize(Simulation, _controller);

            PlayerProgression progression = _controller.PlayerProgression;
            PlayerStatUtil.ApplyHealth(_controller.Health, progression.Table.Get(progression.Level.Value));
            progression.AppliedLevel = progression.Level.Value;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && ServerLoop.Instance != null)
            {
                ServerLoop.Instance.Unregister(this);
            }
        }

        // NGO가 아무 때나 부른다 → 기록만 (deferment)
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void InputRPC(InputBundle bundle)
        {
            if (Inputs.Received.Count >= InputQueue.MAX_RECEIVED) return;
            Inputs.Received.Add(bundle);
        }
    }
}