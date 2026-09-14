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
        int _emptyTicks;
        bool _jumping;
        const int NO_INPUT_THRESHOLD = 5;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _controller.Event.OnAnimationCallback += HandleAnimationCallback;
                _controller.Event.OnAnimationCommit += HandleAnimationCommit;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                _controller.Event.OnAnimationCallback -= HandleAnimationCallback;
                _controller.Event.OnAnimationCommit -= HandleAnimationCommit;
            }
        }

        void FixedUpdate()
        {
            int consume = _queue.Count > 1 ? 2 : 1;
            bool consumed = false;

            for (int i = 0; i < consume; i++)
            {
                if (!TryDequeueInput(out var payload)) break;

                _controller.ApplyPitch(payload.Pitch);
                _controller.Locomotion.ApplyYaw(payload.Yaw);

                if (payload.Jump && _controller.Locomotion.IsGrounded && !_jumping)
                {
                    _jumping = true;
                    _controller.Animation.PlayJump();
                }

                _controller.Simulate(payload);

                _controller.Animation.PlayMove(payload.Move, payload.Run);

                _controller.Client.CreateStateRPC(new StatePayload
                {
                    Tick = payload.Tick,
                    Position = transform.position,
                    VelocityY = _controller.Locomotion.VelocityY,
                });

                consumed = true;
            }

            if (consumed)
            {
                _emptyTicks = 0;
            }
            else
            {
                _emptyTicks++;
                if (_emptyTicks > NO_INPUT_THRESHOLD) _controller.Input.Apply(default);
            }
        }

        [Rpc(SendTo.Server)]
        public void SubmitInputRPC(InputPayload p)
        {
            if (p.Tick <= _lastTick) return;
            _queue[p.Tick] = p;
        }

        public bool TryDequeueInput(out InputPayload p)
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

        void HandleJumpRequest()
        {
            if (_controller.Locomotion.IsGrounded)
            {
                _controller.Animation.PlayJump();
            }
        }

        void HandleAnimationCommit(AnimationID id)
        {
            switch (id)
            {
                case AnimationID.Jump:
                    _controller.Locomotion.Jump(_controller.SettingSO.JumpPower);
                    break;
            }
        }

        void HandleAnimationCallback(AnimationID id)
        {
            switch (id)
            {
                case AnimationID.Jump:
                    _jumping = false;
                    break;
            }
        }
    }
}