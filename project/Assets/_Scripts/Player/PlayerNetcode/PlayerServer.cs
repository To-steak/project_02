using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerServer : NetworkBehaviour
    {
        PlayerController _controller;
        readonly SortedDictionary<int, InputPayload> _queue = new();
        int _previousTick = -1;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        void FixedUpdate()
        {
            int consume = _queue.Count > 1 ? 2 : 1;
            Debug.LogWarning($"{_queue.Count}");
            for (int i = 0; i < consume; i++)
            {
                if (TryDequeue(out InputPayload payload))
                {
                    _controller.Locomotion.Rotate(payload.Yaw);
                    _controller.Locomotion.Simulate(payload, _controller.SettingSO);

                    StatePayload state = _controller.Locomotion.Capture(payload.Tick);
                    _controller.Client.StateRPC(state);
                }
            }
        }

        private bool TryDequeue(out InputPayload payload)
        {
            if (_queue.Count == 0)
            {
                payload = default;
                return false;
            }

            int tick = _queue.Keys.First();
            payload = _queue[tick];
            _queue.Remove(tick);
            _previousTick = tick;
            return true;
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void InputRPC(InputPayload payload)
        {
            if (payload.Tick <= _previousTick) return;
            _queue[payload.Tick] = payload;
        }
    }
}