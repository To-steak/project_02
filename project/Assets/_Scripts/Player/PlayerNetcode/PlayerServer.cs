using Unity.Netcode;
using PlayerState;

namespace PlayerNetcode
{
    public class PlayerServer : NetworkBehaviour
    {
        PlayerController _controller;
        BaseState _state;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _state = _controller.Idle;
            }
        }

        void FixedUpdate()
        {
            _state?.Tick();
            _controller.Locomotion.CheckGrounded(_controller.SettingSO.GroundCheckRadius, _controller.SettingSO.GroundLayer);
            _controller.Locomotion.ApplyGravity(_controller.SettingSO.GravityValue);
            _controller.Locomotion.Move(_controller.Input.Move, _state.MoveSpeed);
            _controller.Locomotion.Rotate(_controller.Input.Look.x, _controller.SettingSO.RotationSpeed);
        }

        public void ChangeState(BaseState state)
        {
            _state.Exit();
            _state = state;
            _state.Enter();
        }
    }
}