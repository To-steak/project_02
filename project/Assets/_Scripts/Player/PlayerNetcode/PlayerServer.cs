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
            _controller.Locomotions.CheckGrounded(_controller.SettingSO.GroundCheckRadius, _controller.SettingSO.GroundLayer);
            _controller.Locomotions.ApplyGravity(_controller.SettingSO.GravityValue);
            _controller.Locomotions.Move(_controller.Inputs.Move, _state.MoveSpeed);
        }

        public void ChangeState(BaseState state)
        {
            _state.Exit();
            _state = state;
            _state.Enter();
        }
    }
}