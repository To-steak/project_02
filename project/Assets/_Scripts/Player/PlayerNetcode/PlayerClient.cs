using Manager;
using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public class PlayerClient : NetworkBehaviour
    {
        PlayerController _controller;

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
                // _controller.Camera.SetAimTargetFromCamera(Camera.main, _controller.SettingSO.HitLayer);
                _controller.Camera.SetAimTargetFromPitch(_controller.Camera.LookPitch);
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
                MoveRPC(_controller.Input.Move);
                LookRPC(_controller.Input.Look, _controller.Camera.LookPitch);
                RunRPC(_controller.Input.Run);
            }
        }

        [Rpc(SendTo.Server)]
        private void MoveRPC(Vector3 move)
        {
            _controller.Input.SetMove(move);
        }

        [Rpc(SendTo.Server)]
        private void LookRPC(Vector2 look, float pitch)
        {
            _controller.Input.SetLook(look);
            _controller.AimPitch.Value = Mathf.Clamp(pitch, _controller.SettingSO.MinPitch, _controller.SettingSO.MaxPitch);
        }

        [Rpc(SendTo.Server)]
        private void RunRPC(bool run)
        {
            _controller.Input.SetRun(run);
        }

        [Rpc(SendTo.Server)]
        private void JumpRPC()
        {
            _controller.Event.RaiseJumpExecute();
        }

        private void HandleJump() => JumpRPC();
    }
}