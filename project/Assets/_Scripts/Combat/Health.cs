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
    public float DamageReduction { get; set; }
    public delegate void DamageHandler(Health health, in DamageInfo info, int previous);
    public event DamageHandler Damaged;
    public event Action<Health> Died;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        DamageReduction = 0.0f;
        Max.Value = _defaultMax;
        Current.Value = _defaultMax;
    }

    public void ApplyDamage(in DamageInfo info)
    {
        if (!IsServer || IsDead || info.Amount <= 0) return;

        int amount = Mathf.RoundToInt(info.Amount * (1.0f - Mathf.Clamp01(DamageReduction)));
        int previous = Current.Value;
        Current.Value = Mathf.Max(0, previous - amount);
        Damaged?.Invoke(this, info, previous);

        if (Current.Value == 0)
        {
            Died?.Invoke(this);
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