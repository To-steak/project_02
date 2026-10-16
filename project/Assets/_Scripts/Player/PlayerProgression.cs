using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // 동기화 프록시: 서버가 정한 Level을 복제 (UI용). 시뮬레이션 반영은 서버 StatSystem → StatePayload
    public class PlayerProgression : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private SOLevelTable _table;
        [SerializeField] private PlayerController _controller;

        public SOLevelTable Table => _table;
        public int AppliedLevel;   // 서버: StatSystem이 마지막으로 반영한 레벨

        protected override void OnNetworkPostSpawn()
        {
            // 스폰 시 1회 초기화 (Authoring). Join이 OnNetworkSpawn에서 넣은 레벨이 이미 있다
            // 클라도 같은 값으로 시작해야 첫 예측이 서버와 맞는다
            int level = Level.Value;
            PlayerStatUtil.Apply(_controller, _table.Get(level), IsServer);
            AppliedLevel = level;
        }
    }
}