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
        Max.Value = _defaultMax;
        Current.Value = _defaultMax;
    }

    public void ApplyDamage(int amount, ulong? attacker)
    {
        if (!IsServer || IsDead || amount <= 0)
        {
            return;
        }

        if (attacker.HasValue)
        {
            _contributors.Add(attacker.Value);
        }

        Current.Value = Mathf.Max(0, Current.Value - amount);
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
