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
        [SerializeField] private PlayerController _controller;

        public SOLevelTable Table => _table;

        protected override void OnNetworkPostSpawn()
        {
            Level.OnValueChanged += OnLevelChanged;
            ApplyLevel(Level.Value);
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
                _health.Current.Value = stats.MaxHp;
            }

            // 예측에 필요하므로 모든 피어에서 (기존 SetMaxEnergy와 같음: 최대치로 회복)
            PlayerSimulation sim = _controller.Simulation;
            sim.MaxEnergy = stats.MaxMp;
            sim.State.Stamina = new StaminaState { Energy = stats.MaxMp, IsExhausted = false };
        }
    }
}