using Unity.Netcode;
using PlayerState;
using System.Collections.Generic;
using PlayerAPI;
using System.Linq;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerServer : NetworkBehaviour
    {
        PlayerController _controller;
        BaseState _state;
        readonly SortedDictionary<int, InputPayload> _queue = new();
        int _lastTick = -1;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _state = _controller.Idle;
                _controller.Event.OnAnimationCallback += () => _state?.OnAnimationCallback();
                _controller.Event.OnAnimationCommit += () => _state?.OnAnimationCommit();
                _controller.Event.OnJumpExecute += () => _state?.OnJump();
            }
        }

        int _starvedTicks;
        void FixedUpdate()
        {
            int consume = _queue.Count > 1 ? 2 : 1;
            bool consumed = false;

            for (int i = 0; i < consume; i++)
            {
                if (!TryDequeueInput(out var payload)) break;

                _controller.AimPitch.Value = Mathf.Clamp(payload.Pitch, _controller.SettingSO.MinPitch, _controller.SettingSO.MaxPitch);
                _controller.Simulate(payload);
                _state?.Tick();
                consumed = true;
            }

            if (consumed)
            {
                _starvedTicks = 0;
            }
            else
            {
                _starvedTicks++;
                if (_starvedTicks > 5) _controller.Input.Apply(default);
            }
        }

        public void ChangeState(BaseState state)
        {
            _state.Exit();
            _state = state;
            _state.Enter();
        }

        [Rpc(SendTo.Server)]
        public void SubmitInputRPC(InputPayload p)
        {
            if (p.Tick <= _lastTick) return;
            _queue[p.Tick] = p;
        }

        public bool TryDequeueInput(out InputPayload p)
        {
            if (_queue.Count == 0) { p = default; return false; }

            var first = _queue.Keys.First();

            Debug.Log($"queue: {_queue.Count}, starved: {_starvedTicks}");
            if (first != _lastTick + 1 && _lastTick >= 0)
            {
                Debug.LogWarning($"consumed gap: {_lastTick} → {first}");
            }

            p = _queue[first];
            _queue.Remove(first);
            _lastTick = first;
            return true;
        }
    }
}