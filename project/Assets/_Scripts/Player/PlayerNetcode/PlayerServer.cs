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
                _controller.Event.OnAnimationCallback += () => _state?.OnAnimationCallback();
                _controller.Event.OnAnimationCommit += () => _state?.OnAnimationCommit();
            }
        }

        void FixedUpdate()
        {
            _controller.Locomotion.CheckGrounded(_controller.SettingSO.GroundCheckRadius, _controller.SettingSO.GroundLayer);
            _controller.Locomotion.ApplyGravity(_controller.SettingSO.GravityValue);

            _state?.Tick();

            _controller.Locomotion.Move(_controller.Input.Move, _state.MoveSpeed);
            _controller.Locomotion.Rotate(_controller.Input.Look.x, _controller.SettingSO.RotationSpeed);

            _controller.AimPitch.Value = _controller.Camera.CalculateServerPitch(
                _controller.AimPitch.Value,
                _controller.Input.Look.y,
                _controller.SettingSO.PitchSpeed,
                _controller.SettingSO.MinPitch,
                _controller.SettingSO.MaxPitch
            );
        }

        public void ChangeState(BaseState state)
        {
            _state.Exit();
            _state = state;
            _state.Enter();
        }
    }
}