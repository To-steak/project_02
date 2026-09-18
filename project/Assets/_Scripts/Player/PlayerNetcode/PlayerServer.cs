using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerServer : NetworkBehaviour
    {
        [SerializeField] private PlayerController _controller;
        private readonly InputPayload[] _inputBuffer = new InputPayload[BUFFER_SIZE];
        private InputPayload _lastInput;
        private int _consumedTick = -1;
        private int _latestTick = -1;
        private const int BUFFER_SIZE = 64;
        private const int BUFFER_MASK = BUFFER_SIZE - 1;

        private void Awake()
        {
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                _inputBuffer[i].Tick = -1;
            }
        }

        private void FixedUpdate()
        {
            int consume = _latestTick - _consumedTick > 1 ? 2 : 1;

            for (int i = 0; i < consume; i++)
            {
                if (!TryDequeue(out InputPayload payload))
                {
                    break;
                }
                _controller.PlayerLocomotion.Rotate(payload.Yaw);
                _controller.PlayerLocomotion.Simulate(payload, _controller.SettingSO);

                StatePayload state = _controller.PlayerLocomotion.Capture(payload.Tick);
                _controller.PlayerClient.StateRPC(state);
            }
        }

        private bool TryDequeue(out InputPayload payload)
        {
            int tick = _consumedTick + 1;
            if (tick > _latestTick)
            {
                payload = default;
                return false;
            }

            InputPayload slot = _inputBuffer[tick & BUFFER_MASK];
            if (slot.Tick == tick)
            {
                payload = slot;
                _lastInput = slot;
            }
            else
            {
                payload = _lastInput;
                payload.Tick = tick;
                payload.Jump = false;
            }

            _consumedTick = tick;
            return true;
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void InputRPC(InputBundle bundle)
        {
            for (int i = 0; i < bundle.Count; i++)
            {
                InputPayload payload = bundle[i];
                if (payload.Tick <= _consumedTick)
                {
                    continue;
                }

                if (payload.Tick > _consumedTick + BUFFER_SIZE)
                {
                    Resync(payload.Tick);
                }

                _inputBuffer[payload.Tick & BUFFER_MASK] = payload;
                if (payload.Tick > _latestTick)
                {
                    _latestTick = payload.Tick;
                }
            }
        }

        private void Resync(int tick)
        {
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                _inputBuffer[i].Tick = -1;
            }

            _consumedTick = tick - 1;
            _latestTick = tick - 1;
        }
    }
}