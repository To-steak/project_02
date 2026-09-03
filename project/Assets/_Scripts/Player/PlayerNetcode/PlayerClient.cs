using Manager;
using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerClient : NetworkBehaviour
    {
        PlayerController _controller;
        int _tick = 0;
        const int BUFFER = 1024;
        readonly InputPayload[] _inputHistory = new InputPayload[BUFFER];
        readonly StatePayload[] _stateHistory = new StatePayload[BUFFER];

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _controller.Event.OnJump += HandleJump;
                _controller.Input.ActiveInputs();
                _controller.Camera.ActiveCamera();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _controller.Event.OnJump -= HandleJump;
                _controller.Input.InactiveInputs();
                CameraManager.Instance.ClearFollowTarget();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Update()
        {
            if (IsOwner)
            {
                _controller.Camera.RotateCamera(_controller.Input.Look.y, _controller.SettingSO.PitchSpeed, _controller.SettingSO.MinPitch, _controller.SettingSO.MaxPitch);
                _controller.Camera.SetAimTargetFromPitch(_controller.Camera.LookPitch);

                MeasureDelay();
            }
            else
            {
                _controller.Camera.SetAimTargetFromPitch(_controller.AimPitch.Value);
            }
        }


        void FixedUpdate()
        {
            if (IsOwner)
            {
                var payload = _controller.Input.Capture(_tick, _controller.Camera.LookPitch);
                _inputHistory[_tick % BUFFER] = payload;

                _controller.Server.SubmitInputRPC(payload);
                _controller.Simulate(payload);

                _stateHistory[_tick % BUFFER] = new StatePayload
                {
                    Tick = _tick,
                    Position = transform.position,
                    RotationY = transform.eulerAngles.y,
                    VelocityY = _controller.Locomotion.VelocityY,
                };

                _tick++;
            }
        }

        [Rpc(SendTo.Server)]
        private void JumpRPC()
        {
            _controller.Event.RaiseJumpExecute();
        }

        private void HandleJump()
        {
            _controller.Animation.PlayJump();
            JumpRPC();
        }


        [Rpc(SendTo.Owner)]
        public void CreateStateRPC(StatePayload payload)
        {
            var predicted = _stateHistory[payload.Tick % BUFFER];
            if (predicted.Tick != payload.Tick) return;

            if (Vector3.Distance(predicted.Position, payload.Position) < 0.1f) return;

            _reconcileCount++;

            Debug.LogWarning($"reconcile at tick {payload.Tick}");

            _controller.Locomotion.RestoreState(payload.Position, payload.RotationY, payload.VelocityY);

            for (int t = payload.Tick + 1; t < _tick; t++)
            {
                _controller.Simulate(_inputHistory[t % BUFFER]);
                _stateHistory[t % BUFFER] = new StatePayload
                {
                    Tick = t,
                    Position = transform.position,
                    RotationY = transform.eulerAngles.y,
                    VelocityY = _controller.Locomotion.VelocityY,
                };

            }

            float error = Vector3.Distance(predicted.Position, payload.Position);
            Debug.LogWarning($"reconcile at tick {payload.Tick}, error {error:F4}, y diff {payload.Position.y - predicted.Position.y:F4}");
        }

        // DEBUG ONLY
        float _lastDelay;
        int _reconcileCount;
        float _moveTime = -1f;
        Vector3 _moveStartPos;
        bool _wasMoving;

        void OnGUI()
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

            GUI.Label(new Rect(xPos, 10, width, height), $"delay: {_lastDelay:F1}ms", style);
            GUI.Label(new Rect(xPos, 30, width, height), $"reconcile: {_reconcileCount}", style);
            GUI.Label(new Rect(xPos, 50, width, height), $"tick: {_tick}", style);
            GUI.Label(new Rect(xPos, 70, width, height), $"rtt: {NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId)}ms", style);
            GUI.Label(new Rect(xPos, 90, width, height), $"fps: {fps:F1}", style);
        }

        void MeasureDelay()
        {
            bool moving = _controller.Input.Move != Vector3.zero;

            if (moving && !_wasMoving)
            {
                _moveTime = Time.realtimeSinceStartup;
                _moveStartPos = transform.position;
            }
            _wasMoving = moving;

            if (_moveTime > 0f)
            {
                Vector3 d = transform.position - _moveStartPos;
                d.y = 0f;
                if (d.sqrMagnitude > 0.01f * 0.01f)
                {
                    _lastDelay = (Time.realtimeSinceStartup - _moveTime) * 1000f;
                    _moveTime = -1f;
                }
            }
        }
    }
}