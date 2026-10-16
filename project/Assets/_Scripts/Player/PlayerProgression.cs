using Project02.Combat;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // 동기화 프록시: 서버가 정한 Level을 모든 피어에 복제하고, 바뀌면 스탯에 반영
    // (클라도 예측용 최대 기력이 필요해서 모든 피어에서 ApplyLevel)
    public class PlayerProgression : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private SOLevelTable _table;
        [SerializeField] private Health _health;
        [SerializeField] private PlayerLocomotion _locomotion;

        public SOLevelTable Table => _table;

        protected override void OnNetworkPostSpawn()
        {
            Level.OnValueChanged += OnLevelChanged;
            ApplyLevel(Level.Value);   // Join이 OnNetworkSpawn에서 넣은 값. 처음 값은 콜백이 안 불리니 직접
        }

        public override void OnNetworkDespawn()
        {
            Level.OnValueChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int previous, int current) => ApplyLevel(current);

        private void ApplyLevel(int level)
        {
            SOLevelTable.Row stats = _table.Get(level);

            if (IsServer)
            {
                _health.Max.Value = stats.MaxHp;
                _health.Current.Value = stats.MaxHp;   // 레벨업 시 완전 회복 (기존 동작)
            }

            _locomotion.SetMaxEnergy(stats.MaxMp);
        }
    }
}