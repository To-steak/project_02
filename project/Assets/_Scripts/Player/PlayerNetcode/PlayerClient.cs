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
                _controller.Input.ActiveInputs();
                _controller.Camera.GetCamera();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _controller.Input.InactiveInputs();
                _controller.Camera.ReleaseCamera();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Update()
        {
            if (IsOwner)
            {
                _controller.Camera.RotateCamera(
                    _controller.Input.Look.y,
                    _controller.SettingSO.PitchSpeed,
                    _controller.SettingSO.MinPitch,
                    _controller.SettingSO.MaxPitch);
            }
        }

        void FixedUpdate()
        {
            if (IsOwner)
            {
                MoveRPC(_controller.Input.Move);
                LookRPC(_controller.Input.Look);
                RunRPC(_controller.Input.Run);

                if (_controller.Input.Jump)
                {
                    JumpRPC(true);
                    _controller.Input.SetJump(false);
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void MoveRPC(Vector3 move)
        {
            _controller.Input.SetMove(move);
        }

        [Rpc(SendTo.Server)]
        private void LookRPC(Vector2 look)
        {
            _controller.Input.SetLook(look);
        }

        [Rpc(SendTo.Server)]
        private void RunRPC(bool run)
        {
            _controller.Input.SetRun(run);
        }

        [Rpc(SendTo.Server)]
        private void JumpRPC(bool jump)
        {
            _controller.Input.SetJump(jump);
        }
    }
}