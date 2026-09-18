using UnityEngine;
using PlayerNetcode;

namespace PlayerAPI
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }

        [SerializeField] private Transform _groundChecker;
        [SerializeField] private CharacterController _character;

        private Vector3 _velocity;

        private void Move(Vector3 direction, float speed)
        {
            _character.Move((direction * speed + Vector3.up * _velocity.y) * Time.fixedDeltaTime);
        }

        private void CheckGrounded(float radius, LayerMask layer)
        {
            IsGrounded = Physics.CheckSphere(_groundChecker.position, radius, layer);

            if (IsGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }
        }

        public void Rotate(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void ApplyGravity(float gravity)
        {
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        public void Jump(float power)
        {
            if (!IsGrounded)
            {
                return;
            }

            _velocity.y = power;
        }

        public StatePayload Capture(int tick) => new StatePayload
        {
            Tick = tick,
            Position = transform.position,
            VelocityY = _velocity.y,
            IsGrounded = IsGrounded
        };

        public void RollbackState(StatePayload payload)
        {
            _character.enabled = false;

            transform.position = payload.Position;
            _velocity.y = payload.VelocityY;
            IsGrounded = payload.IsGrounded;

            _character.enabled = true;
        }

        public bool Simulate(InputPayload payload, PlayerSettingSO setting)
        {
            CheckGrounded(setting.GroundCheckRadius, setting.GroundLayer);

            bool jumped = payload.Jump && IsGrounded;
            if (jumped) Jump(setting.JumpPower);

            ApplyGravity(setting.GravityValue);

            float speed = payload.Move == Vector2.zero ? 0f : (payload.Run ? setting.RunSpeed : setting.WalkSpeed);
            Vector3 direction = Quaternion.Euler(0f, payload.Yaw, 0f) * new Vector3(payload.Move.x, 0f, payload.Move.y);
            Move(direction, speed);

            return jumped;
        }
    }
}