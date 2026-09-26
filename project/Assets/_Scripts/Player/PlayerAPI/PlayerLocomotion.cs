using UnityEngine;
using PlayerNetcode;

namespace PlayerAPI
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public CharacterState State => _state;

        private CharacterState _state;

        public void Rotate(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public StatePayload Capture(int tick) => new StatePayload
        {
            Tick = tick,
            Position = transform.position,
            VerticalSpeed = _state.VerticalSpeed,
            IsGrounded = _state.IsGrounded
        };

        public void RollbackState(StatePayload payload)
        {
            transform.position = payload.Position;
            _state.VerticalSpeed = payload.VerticalSpeed;
            _state.IsGrounded = payload.IsGrounded;
        }

        public void Simulate(InputPayload payload, PlayerSettingSO setting)
        {
            float time = Time.fixedDeltaTime;
            Vector2 move = Vector2.ClampMagnitude(payload.Move, 1.0f);
            float moveSpeed = move == Vector2.zero ? 0.0f : (payload.Run ? setting.RunSpeed : setting.WalkSpeed);
            Vector3 direction = Quaternion.Euler(0.0f, payload.Yaw, 0.0f) * new Vector3(move.x, 0.0f, move.y);
            float jumpSpeed = payload.Jump ? setting.JumpSpeed : 0.0f;

            _state.Position = transform.position;
            _state = CharacterMotor.Step(_state, moveSpeed * time * direction, jumpSpeed, setting.Profile, time);
            transform.position = _state.Position;
        }
    }
}