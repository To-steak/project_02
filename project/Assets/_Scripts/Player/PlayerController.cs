using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Project02.Character;
using Project02.Combat;

namespace Project02.Player
{
    // Authoring: 형제 컴포넌트 참조 + 동기화 변수 + PlayerSimulation 소유
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] public SOPlayer PlayerSettings;
        [SerializeField] public PlayerAnimation PlayerAnimation;
        [SerializeField] public PlayerLocomotion PlayerLocomotion;
        [SerializeField] public PlayerCamera PlayerCamera;
        [SerializeField] public PlayerVisual PlayerVisual;
        [SerializeField] public PlayerServer PlayerServer;
        [SerializeField] public PlayerOwner PlayerOwner;
        [SerializeField] public PlayerRemote PlayerRemote;
        [SerializeField] public PlayerWeapon PlayerWeapon;
        [SerializeField] public PlayerProgression PlayerProgression;
        [SerializeField] public Health Health;

        public readonly PlayerSimulation Simulation = new();

        public readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            Simulation.Movement = PlayerLocomotion.Settings;
            Simulation.Catalog = PlayerWeapon.Catalog;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                EquippedWeapon.Value = 0;   // 기본 무기. 닉네임은 SessionSystem.Join이 넣는다
            }

            EquippedWeapon.OnValueChanged += OnEquippedWeaponChanged;
            PlayerWeapon.Equip(EquippedWeapon.Value, IsClient);   // 연출(모델·IK)만
        }

        protected override void OnNetworkPostSpawn()
        {
            // 스폰 시 1회 초기화 (Authoring). 각 필드는 담당이 하나씩:
            // 위치 = 여기, 무기 = PlayerEquipUtil, 스태미나 = PlayerProgression(PlayerStatUtil)
            Simulation.State.Movement = new MovementState { Position = transform.position };
            PlayerEquipUtil.Apply(Simulation, EquippedWeapon.Value);
        }

        public override void OnNetworkDespawn()
        {
            EquippedWeapon.OnValueChanged -= OnEquippedWeaponChanged;
        }

        // 연출만 (규칙 8): 시뮬레이션 무기는 EquipSystem → StatePayload → Restore
        private void OnEquippedWeaponChanged(int previous, int current)
        {
            PlayerWeapon.Equip(current, IsClient);
        }
    }
}