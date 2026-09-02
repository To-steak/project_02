using System.Collections.Generic;
using Manager;
using PlayerAPI;
using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerClient : NetworkBehaviour
    {
        PlayerController _controller;
        readonly List<InputPayload> _history = new();
        const int SEND_COUNT = 3;

        // DEBUG ONLY
        float _moveTime = -1f;
        Vector3 _moveStartPos;
        bool _wasMoving = false;

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
            }
            else
            {
                _controller.Camera.SetAimTargetFromPitch(_controller.AimPitch.Value);
            }

            // DEBUG ONLY
            if (!IsOwner) return;

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
                    Debug.Log($"input→move: {(Time.realtimeSinceStartup - _moveTime) * 1000f:F1}ms");
                    _moveTime = -1f;
                }
            }
        }

        int tick = 0;
        void FixedUpdate()
        {
            if (IsOwner)
            {
                // int tick = NetworkManager.NetworkTickSystem.LocalTime.Tick;
                var payload = _controller.Input.Capture(tick, _controller.Camera.LookPitch);
                _controller.Server.SubmitInputRPC(payload);
                tick++;

                // int tick = NetworkManager.NetworkTickSystem.LocalTime.Tick;
                // _history.Add(_controller.Input.Capture(tick, _controller.Camera.LookPitch));

                // if (_history.Count > SEND_COUNT)
                // {
                //     _history.RemoveAt(0);
                // }

                // SubmitInputRPC(_history.ToArray());
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


        // DEBUG ONLY
        // int _lastTick = -1;
        // [Rpc(SendTo.Server)]
        // void SubmitInputRPC(InputPayload payload)
        // {
        //     if (payload.Tick <= _lastTick)
        //     {
        //         Debug.LogWarning($"out of order or duplicate: {payload.Tick} after {_lastTick}");
        //     }
        //     else if (payload.Tick > _lastTick + 1)
        //     {
        //         Debug.LogWarning($"gap: {_lastTick} → {payload.Tick}");
        //     }

        //     _controller.AimPitch.Value = Mathf.Clamp(payload.Pitch, _controller.SettingSO.MinPitch, _controller.SettingSO.MaxPitch);
        //     _controller.Input.Apply(payload);
        //     _lastTick = payload.Tick;
        // }
    }
}