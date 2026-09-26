using GameInterface;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PlayerNetcode
{
    public class PlayerClient : NetworkBehaviour
    {
        [SerializeField] private PlayerController _controller;
        private int _tick = 0;

        private const int BUFFER_SIZE = 1024;
        private const int BUFFER_MASK = BUFFER_SIZE - 1;
        private const float THRESHOLD = 0.1f;

        private readonly InputPayload[] _inputHistory = new InputPayload[BUFFER_SIZE];
        private readonly StatePayload[] _stateHistory = new StatePayload[BUFFER_SIZE];

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _controller.PlayerInput.Initialize(new PlayerAction());
                _controller.PlayerInput.Active();
                _controller.PlayerInput.Enable();
                _controller.PlayerCamera.Initialize(GameServices.Camera);
                _controller.PlayerVisual.Initialzie(transform.position);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _controller.PlayerInput.Inactive();
                _controller.PlayerInput.Disable();
                _controller.PlayerCamera.Release();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                _controller.PlayerInput.Destroy();
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                _controller.PlayerCamera.RotatePitch(_controller.PlayerInput.LookInput.y, _controller.PlayerSettings.PitchSpeed, _controller.PlayerSettings.MinPitch, _controller.PlayerSettings.MaxPitch);
                _controller.Pitch.Value = _controller.PlayerCamera.Pitch;
                _controller.PlayerCamera.RotateYaw(_controller.PlayerInput.LookInput.x, _controller.PlayerSettings.RotationSpeed);

                _controller.PlayerLocomotion.Rotate(_controller.PlayerCamera.Yaw);
                _controller.PlayerCamera.ApplyAim(_controller.PlayerInput.AimInput);

                var motion = _controller.PlayerLocomotion.State;
                _controller.PlayerAnimation.SetAirborne(motion.IsGrounded, motion.VerticalSpeed);
                _controller.PlayerAnimation.SetMoveBlendTree(_controller.PlayerInput.MoveInput, _controller.PlayerInput.RunInput, Time.deltaTime);
            }
            else
            {
                _controller.PlayerCamera.ApplyAimTarget(_controller.Pitch.Value);
            }
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                InputPayload input = _controller.PlayerInput.Capture(_tick, _controller.PlayerCamera.Yaw);
                _inputHistory[_tick & BUFFER_MASK] = input;

                _controller.PlayerLocomotion.Simulate(input, _controller.PlayerSettings);

                StatePayload state = _controller.PlayerLocomotion.Capture(_tick);
                _stateHistory[_tick & BUFFER_MASK] = state;

                _controller.PlayerVisual.Record();

                int count = Mathf.Min(InputBundle.CAPACITY, _tick + 1);
                InputBundle bundle = new InputBundle { Count = (byte)count };
                for (int i = 0; i < count; i++)
                {
                    bundle.Set(i, _inputHistory[(_tick - i) & BUFFER_MASK]);

                }
                _controller.PlayerServer.InputRPC(bundle);

                _tick++;
            }
        }

        private void LateUpdate()
        {
            if (IsOwner)
            {
                _controller.PlayerVisual.Interpolate();
            }
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
        public void StateRPC(StatePayload payload)
        {
            var predicted = _stateHistory[payload.Tick & BUFFER_MASK];
            if (predicted.Tick != payload.Tick)
            {
                return;
            }

            if (Vector3.Distance(predicted.Position, payload.Position) >= THRESHOLD)
            {
                DEBUG_RECONCILE++;

                _controller.PlayerLocomotion.RollbackState(payload);

                for (int tick = payload.Tick + 1; tick < _tick; tick++)
                {
                    _controller.PlayerLocomotion.Simulate(_inputHistory[tick & BUFFER_MASK], _controller.PlayerSettings);
                    _stateHistory[tick & BUFFER_MASK] = _controller.PlayerLocomotion.Capture(tick);
                }

                Debug.LogWarning($"reconcile at tick {payload.Tick}, error {Vector3.Distance(predicted.Position, payload.Position):F4}, y diff {payload.Position.y - predicted.Position.y:F4}");
            }
        }

        private int DEBUG_RECONCILE;
        private void OnGUI()
        {
            if (!IsOwner) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.UpperRight;

            float width = 300f;
            float height = 20f;
            float paddingRight = 10f;
            float xPos = Screen.width - width - paddingRight;
            float fps = 1.0f / Time.unscaledDeltaTime;

            GUI.Label(new Rect(xPos, 30, width, height), $"reconcile: {DEBUG_RECONCILE}", style);
            GUI.Label(new Rect(xPos, 50, width, height), $"tick: {_tick}", style);
            GUI.Label(new Rect(xPos, 70, width, height), $"rtt: {NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId)}ms", style);
            GUI.Label(new Rect(xPos, 90, width, height), $"fps: {fps:F1}", style);
        }
    }
}