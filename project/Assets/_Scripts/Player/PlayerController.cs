using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Collections;
using Project02.Character;
using Project02.Combat;

namespace Project02.Player
{
    // Authoring: 형제 컴포넌트 참조 + 동기화 변수 + PlayerSim 소유
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
        [SerializeField] private NetworkTransform _networkTransform;

        public readonly PlayerSimulation Simulation = new();

        public readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            Simulation.Movement = PlayerLocomotion.Settings;
        }

        public override void OnNetworkSpawn()
        {
            EquippedWeapon.OnValueChanged += OnEquippedWeaponChanged;
            Equip(EquippedWeapon.Value);

            if (IsServer)
            {
                EquippedWeapon.Value = 0;   // 닉네임은 SessionSystem.Join이 넣는다
            }
        }

        protected override void OnNetworkPostSpawn()
        {
            _networkTransform.enabled = !(IsOwner && !IsServer);

            // 위치만 초기화. 기력은 PlayerProgression.ApplyLevel, 무기 상태는 Equip이 맡는다
            // (PostSpawn 순서가 컴포넌트 순서라 State 전체를 덮으면 기력이 지워질 수 있음)
            Simulation.State.Movement = new MovementState { Position = transform.position };
        }

        public override void OnNetworkDespawn()
        {
            EquippedWeapon.OnValueChanged -= OnEquippedWeaponChanged;
        }

        private void OnEquippedWeaponChanged(int previous, int current) => Equip(current);

        private void Equip(int id)
        {
            PlayerWeapon.Equip(id, IsClient);
            Simulation.Weapon = PlayerWeapon.Settings;
            Simulation.State.Weapon = default;
        }
    }
}