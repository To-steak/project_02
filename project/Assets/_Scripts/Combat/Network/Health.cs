using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Combat
{
    public class Health : NetworkBehaviour
    {
        [SerializeField] private int _defaultMax = 100;

        public readonly NetworkVariable<int> Current = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<int> Max = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public float DamageReduction;
        public readonly List<ulong> Contributors = new();

        public bool IsDead => Current.Value <= 0;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            // 초기화 (Authoring)
            DamageReduction = 0.0f;
            Contributors.Clear();
            Max.Value = _defaultMax;
            Current.Value = _defaultMax;
        }
    }
}