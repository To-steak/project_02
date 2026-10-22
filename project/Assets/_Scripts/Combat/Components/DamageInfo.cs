using UnityEngine;

namespace Project02.Combat
{
    public enum DamageSource : byte
    {
        Player, // Attacker Id = 클라이언트 ID
        Enemy, // Attacker Id = NetworkObject ID
    }

    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly DamageSource Source;
        public readonly ulong AttackerId;
        public readonly Vector3 Origin;
        public readonly bool IsCritical;

        public bool IsFromPlayer => Source == DamageSource.Player;

        private DamageInfo(int amount, DamageSource source, ulong attackerId, Vector3 origin, bool isCritical)
        {
            Amount = amount;
            Source = source;
            AttackerId = attackerId;
            Origin = origin;
            IsCritical = isCritical;
        }

        public static DamageInfo FromPlayer(int amount, ulong clientId, Vector3 origin, bool isCritical) => new(amount, DamageSource.Player, clientId, origin, isCritical);
        // 공격자 종류를 모르는 쪽(ProjectileSystem 등)이 쓴다. 종류별 규칙(적은 크리티컬 없음)은 위 팩토리를 따른다
        public static DamageInfo From(DamageSource source, int amount, ulong attackerId, Vector3 origin, bool isCritical) =>
            source == DamageSource.Player ? FromPlayer(amount, attackerId, origin, isCritical) : FromEnemy(amount, attackerId, origin, isCritical);
        public static DamageInfo FromEnemy(int amount, ulong networkObjectId, Vector3 origin, bool isCritical) => new(amount, DamageSource.Enemy, networkObjectId, origin, false);
    }
}