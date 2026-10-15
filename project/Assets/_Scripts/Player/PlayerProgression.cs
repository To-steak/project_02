using Project02.Combat;
using Project02.Service;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerProgression : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private SOLevelTable _table;
        [SerializeField] private Health _health;
        [SerializeField] private PlayerLocomotion _locomotion;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Level.Value = _table.GetLevel(SessionService.Instance.GetExp(OwnerClientId));
                SessionService.Instance.ExpChanged += OnExpChanged;
            }
        }

        protected override void OnNetworkPostSpawn()
        {
            Level.OnValueChanged += OnLevelChanged;
            ApplyLevel(Level.Value);   // 처음 값은 OnValueChanged가 안 불리니 직접
        }

        public override void OnNetworkDespawn()
        {
            Level.OnValueChanged -= OnLevelChanged;
            if (IsServer && SessionService.Instance != null)
            {
                SessionService.Instance.ExpChanged -= OnExpChanged;
            }
        }

        // 서버: 경험치가 바뀌면 레벨만 바꾼다. 나머지는 ApplyLevel이 처리
        private void OnExpChanged(ulong clientId, int exp)
        {
            if (clientId != OwnerClientId)
            {
                return;
            }

            Level.Value = _table.GetLevel(exp);
        }

        private void OnLevelChanged(int previous, int current)
        {
            ApplyLevel(current);
        }

        private void ApplyLevel(int level)
        {
            SOLevelTable.Row stats = _table.Get(level);
            _health.SetMax(stats.MaxHp);
            _locomotion.SetMaxEnergy(stats.MaxMp);
        }
    }
}