using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

namespace PlayerNetcode
{
    public class PlayerServer : NetworkBehaviour
    {
        PlayerController _controller;
        readonly SortedDictionary<int, InputPayload> _queue = new();
        int _lastTick = -1;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        void FixedUpdate()
        {
            int consume = _queue.Count > 1 ? 2 : 1;

            for (int i = 0; i < consume; i++)
            {
                if (!TryDequeueInput(out var payload)) break;

                _controller.ApplyPitch(payload.Pitch);
                _controller.Locomotion.ApplyYaw(payload.Yaw);

                _controller.Simulate(payload);

                _controller.Client.CreateStateRPC(new StatePayload
                {
                    Tick = payload.Tick,
                    Position = transform.position,
                    VelocityY = _controller.Locomotion.VelocityY,
                });

            }
        }

        [Rpc(SendTo.Server)]
        public void SubmitInputRPC(InputPayload p)
        {
            if (p.Tick <= _lastTick) return;
            _queue[p.Tick] = p;
        }

        private bool TryDequeueInput(out InputPayload p)
        {
            if (_queue.Count == 0)
            {
                p = default;
                return false;
            }

            var first = _queue.Keys.First();
            p = _queue[first];
            _queue.Remove(first);
            _lastTick = first;
            return true;
        }
    }
}