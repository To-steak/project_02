using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerRemote : NetworkBehaviour
    {
        [SerializeField] private PlayerController _controller;

        private float _previous;
        private float _current;

        public override void OnNetworkSpawn()
        {
            enabled = IsClient && !IsOwner;
            if (!enabled)
            {
                return;
            }

            _previous = _current = _controller.Pitch.Value;
        }

        private void Update()
        {
            float pitch = Mathf.Lerp(_previous, _current, PlayerVisual.Alpha);
            _controller.PlayerVisual.ApplyPitch(pitch);
            _controller.PlayerVisual.AimAtPitch(pitch);
        }

        private void FixedUpdate()
        {
            _previous = _current;
            _current = _controller.Pitch.Value;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void FireRPC(Vector3 aimPoint, Vector2 spread)
        {
            if (IsOwner)
            {
                return;
            }

            _controller.PlayerWeapon.View.SpawnBullet(aimPoint, spread);
        }
    }
}