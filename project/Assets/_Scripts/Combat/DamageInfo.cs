using UnityEngine;

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
    public static DamageInfo FromEnemy(int amount, ulong networkObjectId, Vector3 origin, bool isCritical) => new(amount, DamageSource.Enemy, networkObjectId, origin, false);
}