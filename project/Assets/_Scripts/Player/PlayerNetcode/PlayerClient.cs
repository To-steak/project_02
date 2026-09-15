using Manager;
using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerClient : NetworkBehaviour
    {
        PlayerController _controller;
        int _tick = 0;

        const int BUFFER_SIZE = 1024;
        const float THRESHOLD = 0.1f;

        readonly InputPayload[] _inputHistory = new InputPayload[BUFFER_SIZE];
        readonly StatePayload[] _stateHistory = new StatePayload[BUFFER_SIZE];

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _controller.Input.ActiveInputs();
                _controller.Camera.ActiveCamera();
                _controller.Visual.ActiveVisual();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _controller.Input.InactiveInputs();
                _controller.Visual.InactiveVisual();
                CameraManager.Instance.ReleaseTarget();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Update()
        {
            if (IsOwner)
            {
                _controller.Camera.RotatePitch(_controller.Input.Look.y, _controller.SettingSO.PitchSpeed, _controller.SettingSO.MinPitch, _controller.SettingSO.MaxPitch);
                _controller.Pitch.Value = _controller.Camera.Pitch;
                _controller.Camera.RotateYaw(_controller.Input.Look.x, _controller.SettingSO.RotationSpeed);

                _controller.Locomotion.Rotate(_controller.Camera.Yaw);
                _controller.Camera.ApplyAim(_controller.Input.Aim);
            }
            else
            {
                _controller.Camera.ApplyAimTarget(_controller.Pitch.Value);
            }
        }


        void FixedUpdate()
        {
            if (IsOwner)
            {
                InputPayload input = _controller.Input.Capture(_tick, _controller.Camera.Yaw);
                _inputHistory[_tick % BUFFER_SIZE] = input;

                if (_controller.Locomotion.Simulate(input, _controller.SettingSO)) _controller.Animation.PlayJump();
                _controller.Animation.PlayMove(input.Move, input.Run);

                StatePayload state = _controller.Locomotion.Capture(_tick);
                _stateHistory[_tick % BUFFER_SIZE] = state;

                _controller.Visual.Record();
                _controller.Server.InputRPC(input);

                _tick++;
            }
        }

        void LateUpdate()
        {
            if (IsOwner)
            {
                _controller.Visual.Interpolate();
            }
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
        public void StateRPC(StatePayload payload)
        {
            var predicted = _stateHistory[payload.Tick % BUFFER_SIZE];
            if (predicted.Tick != payload.Tick)
            {
                return;
            }

            if (Vector3.Distance(predicted.Position, payload.Position) >= THRESHOLD)
            {
                Vector3 before = _controller.Visual.GetVisualPosition();
                _controller.Locomotion.RollbackState(payload);

                for (int tick = payload.Tick + 1; tick < _tick; tick++)
                {
                    _controller.Locomotion.Simulate(_inputHistory[tick % BUFFER_SIZE], _controller.SettingSO);
                    _stateHistory[tick % BUFFER_SIZE] = _controller.Locomotion.Capture(tick);
                }

                _controller.Visual.SetOffset(before);
                Debug.LogWarning($"reconcile at tick {payload.Tick}, error {Vector3.Distance(predicted.Position, payload.Position):F4}, y diff {payload.Position.y - predicted.Position.y:F4}");
            }
        }
    }
}