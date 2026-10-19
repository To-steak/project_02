using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // 동기화 프록시: 서버가 정한 Level을 복제 (UI용). 시뮬레이션 반영은 서버 StatSystem → StatePayload
    public class PlayerProgression : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private SOLevelTable _table;

        public SOLevelTable Table => _table;
        public int AppliedLevel;   // 서버: 마지막으로 스탯에 반영한 레벨 (PlayerServerAuthoring 스폰, StatSystem)
    }
}