using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // 엔티티당 하나: 이 플레이어의 모든 RPC·NetworkVariable과 수신함. 아무것도 참조하지 않는다
    // RPC 처리기는 수신함에 기록만 한다 (규칙 8). 처리는 각 World의 시스템
    public class PlayerNetwork : NetworkBehaviour
    {
        public const int MAX_RECEIVED_INPUTS = 16;   // 한 프레임에 쌓일 수 있는 번들 상한 (스팸 방어)
        public const int MAX_FIRES = 16;             // 스냅샷이 끊겨 RenderTick이 멈춰도 무한히 쌓이지 않게

        // ── 동기화 값 (서버만 쓴다) ──
        public readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);   // SessionSystem이 경험치로 정한다. 스탯 반영은 StatSystem
        
        // ── 수신함: 서버 (ServerPlayerSystem이 비운다) ──
        public readonly List<InputBundle> ReceivedInputs = new();

        // ── 수신함: 소유자 (ReconcileSystem이 비운다) ──
        public StatePayload PendingState;
        public bool HasPendingState;
        public int LatestServerTick = -1;

        // ── 수신함: 원격 클라 (RemotePlayerViewSystem이 비운다) ──
        public readonly List<RemoteFire> Fires = new();

        public override void OnNetworkSpawn()
        {
            ReceivedInputs.Clear();
            HasPendingState = false;
            LatestServerTick = -1;
            Fires.Clear();

            if (IsServer)
            {
                EquippedWeapon.Value = 0;   // 기본 무기. 닉네임은 SessionSystem.Join이 넣는다
            }
        }

        // 소유자 → 서버
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void InputRPC(InputBundle bundle)
        {
            if (ReceivedInputs.Count >= MAX_RECEIVED_INPUTS) return;
            ReceivedInputs.Add(bundle);
        }

        // 서버 → 소유자
        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
        public void StateRPC(StatePayload payload)
        {
            if (payload.State.Tick <= LatestServerTick) return;   // 늦게 온 옛 상태 버림
            LatestServerTick = payload.State.Tick;

            PendingState = payload;
            HasPendingState = true;
        }

        // 서버 → 다른 클라 (발사 연출)
        [Rpc(SendTo.ClientsAndHost)]
        public void FireRPC(int tick, Vector3 aimPoint, Vector2 spread)
        {
            if (IsOwner) return;   // 소유자는 예측으로 이미 재생함

            if (Fires.Count >= MAX_FIRES) Fires.RemoveAt(0);   // 가장 오래된 것 버림
            Fires.Add(new RemoteFire(tick, aimPoint, spread));
        }
    }
}