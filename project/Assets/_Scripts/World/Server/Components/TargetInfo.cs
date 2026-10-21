using UnityEngine;

namespace Project02.World
{
    // 적이 볼 수 있는 대상 한 명. TargetSystem.Collect가 매 틱 world.Targets에 채운다
    public readonly struct TargetInfo
    {
        public readonly ulong ClientId;
        public readonly Vector3 Position;
        public readonly bool IsDead;

        public TargetInfo(ulong clientId, Vector3 position, bool isDead)
        {
            ClientId = clientId;
            Position = position;
            IsDead = isDead;
        }
    }
}