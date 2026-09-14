using UnityEngine;

namespace PlayerAPI
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }
        public float VelocityY => _velocity.y;
        public float Yaw { get; private set; }

        [SerializeField] Transform groundChecker;

        CharacterController _character;
        Vector3 _velocity;

        public void Initialize()
        {
            _character = GetComponent<CharacterController>();
        }

        public void Move(Vector3 move, float speed, float yaw)
        {
            Vector3 direction = Quaternion.Euler(0f, yaw, 0f) * new Vector3(move.x, 0f, move.z);
            _character.Move((direction * speed + Vector3.up * _velocity.y) * Time.fixedDeltaTime);
        }

        public void CheckGrounded(float radius, LayerMask layer)
        {
            IsGrounded = Physics.CheckSphere(groundChecker.position, radius, layer);

            if (IsGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }
        }

        public void ApplyGravity(float gravity)
        {
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        public void ApplyYaw(float yaw)
        {
            Yaw = yaw;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        public void Jump(float power)
        {
            if (!IsGrounded)
            {
                return;
            }

            _velocity.y = power;
        }

        public void RestoreState(Vector3 position, float velocityY)
        {
            _character.enabled = false;
            transform.position = position;
            _character.enabled = true;
            _velocity.y = velocityY;
        }
    }
}