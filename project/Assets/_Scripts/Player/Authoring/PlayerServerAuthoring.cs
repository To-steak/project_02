using Unity.Netcode;
using UnityEngine;
using Project02.Combat;
using Project02.World;

namespace Project02.Player
{
    // Authoring (서버 World): 등록, 서버 사본 소유. RPC는 PlayerNetwork
    public class PlayerServerAuthoring : NetworkBehaviour
    {
        public readonly PlayerSimulation Simulation = new();   // 서버 권한 상태
        public readonly InputQueue Inputs = new();
        public readonly RewindTarget Rewind = new();   // 지연 보상 이력. RewindSystem이 기록, ProjectileSystem이 Hittables로 판정
        public int AppliedLevel;   // 마지막으로 스탯에 반영한 레벨. StatSystem이 Network.Level과 비교한다

        public PlayerNetwork Network => _network;
        public Health Health => _health;
        public SOPlayer Settings => _settings;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private Health _health;
        [SerializeField] private SOPlayer _settings;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            ServerLoop.Instance.Register(this);   // SessionSystem.Join → Nickname, Level / Hittables 등록
        }

        protected override void OnNetworkPostSpawn()
        {
            if (!IsServer) return;

            // 스폰 시 1회 (Authoring). Join이 넣은 Level, PlayerNetwork의 EquippedWeapon이 이미 있다
            int level = _network.Level.Value;
            PlayerSpawnUtil.Initialize(Simulation, transform.position, _settings, _network.EquippedWeapon.Value, level);
            PlayerStatUtil.ApplyHealth(_health, _settings.LevelTable.Get(level));
            AppliedLevel = level;

            Rewind.History.Clear();
            Rewind.Radius = _settings.Movement.Profile.Radius;
            Rewind.Height = _settings.Movement.Profile.Height;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && ServerLoop.Instance != null)
            {
                ServerLoop.Instance.Unregister(this);
            }
        }
    }
}