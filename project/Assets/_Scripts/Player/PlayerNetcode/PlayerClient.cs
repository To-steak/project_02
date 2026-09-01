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
                _controller.Input.ActiveInputs();
                CameraManager.Instance.SetFollowTarget(transform);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _controller.Input.InactiveInputs();
                CameraManager.Instance.ClearFollowTarget();
            }
        }

        void FixedUpdate()
        {
            if (IsOwner)
            {
                MoveRPC(_controller.Input.Move);
                LookRPC(_controller.Input.Look);
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
    }
}