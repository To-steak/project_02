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
                _controller.Camera.RotateYaw(_controller.Input.Look.x, _controller.SettingSO.RotationSpeed);

                _controller.Locomotion.ApplyYaw(_controller.Camera.Yaw);
                _controller.Camera.ApplyAim(_controller.Input.Aim);

                MeasureDelay();
            }
            else
            {
                _controller.Camera.ApplyPitch(_controller.Pitch.Value);
            }
        }


        void FixedUpdate()
        {
            if (IsOwner)
            {
                var payload = _controller.Input.Capture(_tick, _controller.Camera.Pitch, _controller.Camera.Yaw);
                _inputHistory[_tick % BUFFER] = payload;

                _controller.Server.SubmitInputRPC(payload);

                if (_controller.Simulate(payload))
                {
                    _controller.Animation.PlayJump();
                }
                _controller.Animation.PlayMove(payload.Move, payload.Run);

                _stateHistory[_tick % BUFFER] = new StatePayload
                {
                    Tick = _tick,
                    Position = transform.position,
                    VelocityY = _controller.Locomotion.VelocityY,
                };

                _controller.Visual.Record();

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

        [Rpc(SendTo.Owner)]
        public void CreateStateRPC(StatePayload payload)
        {
            var predicted = _stateHistory[payload.Tick % BUFFER];
            if (predicted.Tick != payload.Tick)
            {
                return;
            }

            if (Vector3.Distance(predicted.Position, payload.Position) >= 0.1f)
            {
                DEBUG_RECONCILE++;
                Vector3 before = _controller.Visual.CaptureVisualPosition();
                _controller.Locomotion.RestoreState(payload.Position, payload.VelocityY);

                for (int t = payload.Tick + 1; t < _tick; t++)
                {
                    _controller.Simulate(_inputHistory[t % BUFFER]);
                    _stateHistory[t % BUFFER] = new StatePayload
                    {
                        Tick = t,
                        Position = transform.position,
                        VelocityY = _controller.Locomotion.VelocityY,
                    };

                }
                _controller.Visual.AbsorbCorrection(before);
                Debug.LogWarning($"reconcile at tick {payload.Tick}, error {Vector3.Distance(predicted.Position, payload.Position):F4}, y diff {payload.Position.y - predicted.Position.y:F4}");
            }
        }

        // DEBUG ONLY
        float DEBUG_LAST_DELAY;
        int DEBUG_RECONCILE;
        float DEBUG_MOVE_TIEM = -1f;
        Vector3 DEBUG_MOVE_START_POS;
        bool DEBUG_WAS_MOVING;

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

            GUI.Label(new Rect(xPos, 10, width, height), $"delay: {DEBUG_LAST_DELAY:F1}ms", style);
            GUI.Label(new Rect(xPos, 30, width, height), $"reconcile: {DEBUG_RECONCILE}", style);
            GUI.Label(new Rect(xPos, 50, width, height), $"tick: {_tick}", style);
            GUI.Label(new Rect(xPos, 70, width, height), $"rtt: {NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId)}ms", style);
            GUI.Label(new Rect(xPos, 90, width, height), $"fps: {fps:F1}", style);
        }

        void MeasureDelay()
        {
            bool moving = _controller.Input.Move != Vector3.zero;

            if (moving && !DEBUG_WAS_MOVING)
            {
                DEBUG_MOVE_TIEM = Time.realtimeSinceStartup;
                DEBUG_MOVE_START_POS = transform.position;
            }
            DEBUG_WAS_MOVING = moving;

            if (DEBUG_MOVE_TIEM > 0f)
            {
                Vector3 d = transform.position - DEBUG_MOVE_START_POS;
                d.y = 0f;
                if (d.sqrMagnitude > 0.01f * 0.01f)
                {
                    DEBUG_LAST_DELAY = (Time.realtimeSinceStartup - DEBUG_MOVE_TIEM) * 1000f;
                    DEBUG_MOVE_TIEM = -1f;
                }
            }
        }
    }
}