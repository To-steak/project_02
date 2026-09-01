using UnityEngine;

namespace PlayerAPI
{
    public class PlayerLocomotion : MonoBehaviour
    {
        CharacterController _character;
        [SerializeField] Transform groundChecker;
        Vector3 _velocity;
        public bool IsGrounded { get; private set; }

        public void Initialize()
        {
            _character = GetComponent<CharacterController>();
        }

        public void Move(Vector3 move, float speed)
        {
            _character.Move(((move * speed) + _velocity) * Time.fixedDeltaTime);
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
    }
}