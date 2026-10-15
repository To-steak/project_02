using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Collections;

namespace Project02.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] internal SOPlayer PlayerSettings;
        [SerializeField] internal PlayerInput PlayerInput;
        [SerializeField] internal PlayerAnimation PlayerAnimation;
        [SerializeField] internal PlayerLocomotion PlayerLocomotion;
        [SerializeField] internal PlayerCamera PlayerCamera;
        [SerializeField] internal PlayerVisual PlayerVisual;
        [SerializeField] internal PlayerServer PlayerServer;
        [SerializeField] internal PlayerOwner PlayerOwner;
        [SerializeField] internal PlayerRemote PlayerRemote;
        [SerializeField] internal PlayerWeapon PlayerWeapon;
        [SerializeField] internal Health Health;
        [SerializeField] private NetworkTransform _networkTransform;

        internal readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        internal readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        internal readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        internal readonly NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        internal PlayerLevelTable.Row Stats => PlayerSettings.LevelTable.Get(Level.Value);

        public override void OnNetworkSpawn()
        {
            EquippedWeapon.OnValueChanged += OnEquippedWeaponChanged;
            PlayerWeapon.Equip(EquippedWeapon.Value, IsClient);

            if (IsServer)
            {
                EquippedWeapon.Value = 0;
                Nickname.Value = new FixedString64Bytes(SessionService.Instance.GetNickname(OwnerClientId) ?? $"Player#{OwnerClientId}");
                Level.Value = PlayerSettings.LevelTable.GetLevel(SessionService.Instance.GetExp(OwnerClientId));
                SessionService.Instance.ExpChanged += OnExpChanged;
            }
        }

        protected override void OnNetworkPostSpawn()
        {
            PlayerServer.enabled = IsServer;

            _networkTransform.enabled = !(IsOwner && !IsServer);
            if (IsServer)
            {
                Health.SetMax(Stats.MaxHp, true);
            }

            PlayerLocomotion.Initialize(Stats.MaxMp);
        }

        public override void OnNetworkDespawn()
        {
            EquippedWeapon.OnValueChanged -= OnEquippedWeaponChanged;
            if (IsServer && SessionService.Instance != null)
            {
                SessionService.Instance.ExpChanged -= OnExpChanged;
            }
        }

        public bool Step(in InputPayload input, out Shot shot)
        {
            Simulate(input);
            return PlayerWeapon.TryFire(input, PlayerLocomotion.IsRunning, OwnerClientId, out shot);
        }

        public void Simulate(in InputPayload input)
        {
            PlayerLocomotion.Simulate(input, Stats.MaxMp);
        }

        private void OnExpChanged(ulong clientId, int exp)
        {
            if (clientId != OwnerClientId)
            {
                return;
            }

            int level = PlayerSettings.LevelTable.GetLevel(exp);
            if (level == Level.Value)
            {
                return;
            }

            Level.Value = level;
            Health.SetMax(Stats.MaxHp, true);
        }

        private void OnEquippedWeaponChanged(int previous, int current)
        {
            PlayerWeapon.Equip(current, IsClient);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (PlayerSettings != null)
            {
                DrawCapsule();
            }
        }

        private void DrawCapsule()
        {
            float radius = PlayerSettings.Profile.Radius;
            Vector3 position = transform.position;
            LayerMask layer = PlayerSettings.Profile.GroundLayer | PlayerSettings.Profile.ObstacleLayer;

            CharacterPhysics.GetCapsule(position, radius, PlayerSettings.Profile.Height, out Vector3 bottom, out Vector3 top);

            bool overlapped = Physics.CheckCapsule(bottom, top, radius, layer, QueryTriggerInteraction.Ignore);
            Gizmos.color = overlapped ? Color.red : Color.blue;

            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawWireSphere(top, radius);

            Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
            Gizmos.DrawLine(bottom + Vector3.left * radius, top + Vector3.left * radius);
            Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
            Gizmos.DrawLine(bottom + Vector3.back * radius, top + Vector3.back * radius);

            // 올라설 수 있는 최대 턱 높이
            Vector3 step = position + Vector3.up * PlayerSettings.Profile.StepHeight;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(step + Vector3.right * radius, step + Vector3.forward * radius);
            Gizmos.DrawLine(step + Vector3.forward * radius, step + Vector3.left * radius);
            Gizmos.DrawLine(step + Vector3.left * radius, step + Vector3.back * radius);
            Gizmos.DrawLine(step + Vector3.back * radius, step + Vector3.right * radius);

            if (Application.isPlaying)
            {
                Gizmos.color = PlayerLocomotion.State.IsGrounded ? Color.yellow : Color.gray;
                Gizmos.DrawLine(position, position + Vector3.down * (radius * 0.5f));
            }
        }
#endif
    }
}