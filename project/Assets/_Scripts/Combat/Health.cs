using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    [SerializeField] private int _defaultMax = 100;

    public readonly NetworkVariable<int> Current = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public readonly NetworkVariable<int> Max = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool IsDead => Current.Value <= 0;

    /// <summary>
    /// 서버 전용. 받는 피해 감소율(0~1). 예: Defend 중 0.95
    /// </summary>
    public float DamageReduction { get; set; }

    public delegate void DamageHandler(Health health, in DamageInfo info, int previous);

    /// <summary>
    /// 서버에서만 발생하는 피격 이벤트. 사망 피해에도 발생하며 Died보다 먼저 호출된다.
    /// previous는 피해 직전 체력. 실제 피해량은 previous - Current.Value.
    /// </summary>
    public event DamageHandler Damaged;

    /// <summary>
    /// 서버에서만 발생하는 사망 이벤트.
    /// 두 번째 인자는 피해를 준 플레이어들의 클라이언트 ID.
    /// </summary>
    public event Action<Health, ulong[]> Died;

    private readonly HashSet<ulong> _contributors = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        _contributors.Clear();
        DamageReduction = 0.0f;
        Max.Value = _defaultMax;
        Current.Value = _defaultMax;
    }

    public void ApplyDamage(in DamageInfo info)
    {
        if (!IsServer || IsDead || info.Amount <= 0)
        {
            return;
        }

        if (info.IsFromPlayer)
        {
            _contributors.Add(info.AttackerId);
        }

        int amount = Mathf.RoundToInt(info.Amount * (1.0f - Mathf.Clamp01(DamageReduction)));

        int previous = Current.Value;
        Current.Value = Mathf.Max(0, previous - amount);
        Damaged?.Invoke(this, info, previous);

        if (Current.Value == 0)
        {
            ulong[] contributors = new ulong[_contributors.Count];
            _contributors.CopyTo(contributors);
            Died?.Invoke(this, contributors);
        }
    }

    /// <summary>
    /// 레벨 업으로 최대 체력 갱신
    /// </summary>
    /// <param name="max"></param>
    /// <param name="refill"></param>
    public void SetMax(int max, bool refill)
    {
        if (!IsServer)
        {
            return;
        }

        Max.Value = max;
        Current.Value = refill ? max : Mathf.Min(Current.Value, max);
    }
}