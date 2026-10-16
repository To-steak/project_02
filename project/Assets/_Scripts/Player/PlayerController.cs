using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Collections;
using Project02.Combat;
using Project02.Service;

namespace Project02.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] public SOPlayer PlayerSettings;
        [SerializeField] public PlayerInput PlayerInput;
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

        public readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            EquippedWeapon.OnValueChanged += OnEquippedWeaponChanged;
            PlayerWeapon.Equip(EquippedWeapon.Value, IsClient);

            if (IsServer)
            {
                EquippedWeapon.Value = 0;
                Nickname.Value = new FixedString64Bytes(SessionService.Instance.GetNickname(OwnerClientId) ?? $"Player#{OwnerClientId}");
            }
        }

        protected override void OnNetworkPostSpawn()
        {
            _networkTransform.enabled = !(IsOwner && !IsServer);

            PlayerLocomotion.Initialize();
        }

        public override void OnNetworkDespawn()
        {
            EquippedWeapon.OnValueChanged -= OnEquippedWeaponChanged;
        }

        public bool Step(in InputPayload input, float deltaTime, out ShotInfo shot)
        {
            PlayerLocomotion.Simulate(input, deltaTime);
            return PlayerWeapon.TryFire(input, PlayerLocomotion.IsRunning, OwnerClientId, deltaTime, out shot);
        }

        public PlayerSimulationState Capture(int tick) => new PlayerSimulationState
        {
            Tick = tick,
            Movement = PlayerLocomotion.Movement,
            Stamina = PlayerLocomotion.Stamina,
            Weapon = PlayerWeapon.State,
        };

        public void Restore(in PlayerSimulationState state)
        {
            PlayerLocomotion.Restore(state.Movement, state.Stamina);
            PlayerWeapon.Restore(state.Weapon);
        }

        private void OnEquippedWeaponChanged(int previous, int current)
        {
            PlayerWeapon.Equip(current, IsClient);
        }
    }
}