using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Combat
{
    public class Health : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Current = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<int> Max = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public float DamageReduction;
        public readonly List<ulong> Contributors = new();

        public bool IsDead => Current.Value <= 0;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            // 로컬 초기화만. Max/Current는 각 Authoring의 OnNetworkSpawn이 채운다 (쓰기 지점: Authoring, DamageSystem, PlayerStatUtil.ApplyHealth)
            DamageReduction = 0.0f;
            Contributors.Clear();
        }
    }
}