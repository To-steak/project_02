using UnityEngine;

namespace PlayerAPI
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }

        [SerializeField] Transform groundChecker;

        CharacterController _character;
        Vector3 _velocity;

        public void Initialize()
        {
            _character = GetComponent<CharacterController>();
        }

        public void Move(Vector3 move, float speed)
        {
            Vector3 localMove = transform.TransformDirection(move);
            Vector3 horizontal = localMove * speed;

            _character.Move((horizontal + Vector3.up * _velocity.y) * Time.fixedDeltaTime);
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

        public void Rotate(float lookX, float speed)
        {
            transform.Rotate(Vector3.up * lookX * speed * Time.fixedDeltaTime);
        }

        public void Jump(float power)
        {
            if (!IsGrounded)
            {
                return;
            }

            _velocity.y = power;
        }
    }
}