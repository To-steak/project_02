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

        public PlayerNetwork Network => _network;
        public Health Health => _health;
        public PlayerProgression Progression => _progression;
        public SOPlayer Settings => _settings;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private Health _health;
        [SerializeField] private PlayerProgression _progression;
        [SerializeField] private PlayerLocomotion _locomotion;   // SOMovement
        [SerializeField] private PlayerWeaponView _weapon;           // SOCatalog
        [SerializeField] private SOPlayer _settings;             // FireOriginOffset

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            ServerLoop.Instance.Register(this);   // SessionSystem.Join → Nickname, Level
        }

        protected override void OnNetworkPostSpawn()
        {
            if (!IsServer) return;

            // 스폰 시 1회 (Authoring). Join이 넣은 Level, PlayerNetwork의 EquippedWeapon이 이미 있다
            SOLevelTable.Row stats = _progression.Table.Get(_progression.Level.Value);
            PlayerSpawnUtil.Initialize(Simulation, transform.position, _locomotion.Settings, _weapon.Catalog, _network.EquippedWeapon.Value, stats);
            PlayerStatUtil.ApplyHealth(_health, stats);
            _progression.AppliedLevel = _progression.Level.Value;
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