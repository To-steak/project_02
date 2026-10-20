using UnityEngine;

namespace Project02.Player
{
    // 프록시: 애니메이터 파라미터 쓰기. 소유자는 ViewSystem, 원격은 InterpolationSystem이 부른다
    public class PlayerAnimatorView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private readonly int _run = Animator.StringToHash("Run");
        private readonly int _moveX = Animator.StringToHash("MoveX");
        private readonly int _moveY = Animator.StringToHash("MoveY");
        private readonly int _isGrounded = Animator.StringToHash("IsGrounded");
        private readonly int _verticalSpeed = Animator.StringToHash("VerticalSpeed");

        private const float BLEND_TREE_WALK = 0.0f;
        private const float BLEND_TREE_RUN = 1.0f;
        private const float BLEND_TREE_DAMP_TIME = 0.1f;

        public void SetMoveBlendTree(Vector2 move, bool run, float time)
        {
            _animator.SetFloat(_moveX, move.x, BLEND_TREE_DAMP_TIME, time);
            _animator.SetFloat(_moveY, move.y, BLEND_TREE_DAMP_TIME, time);
            _animator.SetFloat(_run, run ? BLEND_TREE_RUN : BLEND_TREE_WALK, BLEND_TREE_DAMP_TIME, time);
        }

        public void SetAirborne(bool grounded, float verticalSpeed)
        {
            _animator.SetBool(_isGrounded, grounded);
            _animator.SetFloat(_verticalSpeed, verticalSpeed);
        }
    }
}