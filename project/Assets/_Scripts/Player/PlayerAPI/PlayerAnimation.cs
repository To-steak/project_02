using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Assertions;
using UnityChan;

namespace PlayerAPI
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private NetworkAnimator _animator;
        private readonly int _run = Animator.StringToHash("Run");
        private readonly int _moveX = Animator.StringToHash("MoveX");
        private readonly int _moveY = Animator.StringToHash("MoveY");
        private readonly int _jump = Animator.StringToHash("Jump");
        private readonly int _isGrounded = Animator.StringToHash("IsGrounded");
        private readonly int _verticalSpeed = Animator.StringToHash("VerticalSpeed");

        private const float BLEND_TREE_WALK = 0.0f;
        private const float BLEND_TREE_RUN = 1.0f;
        private const float BLEND_TREE_DAMP_TIME = 0.1f;

        private const int FACE_LAYER_INDEX = 1;

        [SerializeField] private List<string> _faceStateNames = new List<string>();
        [SerializeField] private int _defaultFaceAnimationIndex = 0;
        private HashSet<string> _faceStateNameSet;

        private void Awake()
        {
            _faceStateNameSet = new HashSet<string>(_faceStateNames);
        }

        public void SetMoveBlendTree(Vector2 move, bool run, float time)
        {
            _animator.Animator.SetFloat(_moveX, move.x, BLEND_TREE_DAMP_TIME, time);
            _animator.Animator.SetFloat(_moveY, move.y, BLEND_TREE_DAMP_TIME, time);
            _animator.Animator.SetFloat(_run, run ? BLEND_TREE_RUN : BLEND_TREE_WALK, BLEND_TREE_DAMP_TIME, time);
        }

        public void PlayJump()
        {
            _animator.Animator.SetTrigger(_jump);
        }

        public void SetAirborne(bool grounded, float verticalSpeed)
        {
            _animator.Animator.SetBool(_isGrounded, grounded);
            _animator.Animator.SetFloat(_verticalSpeed, verticalSpeed);
        }

        private void OnCallChangeFace(string str)
        {
            str = str.Split('@')[0];
            Assert.IsNotNull(_faceStateNameSet);
            if (_faceStateNameSet.Contains(str))
            {
                TryOverrideFaceAnimation(str);
            }
            else
            {
                Assert.IsTrue(_faceStateNames.Count > 0, "No face animation states found in the animator controller.");
                if (_faceStateNames.Count > 0)
                {
                    TryOverrideFaceAnimation(_faceStateNames[_defaultFaceAnimationIndex]);
                }
            }
        }

        private void TryOverrideFaceAnimation(string str)
        {
            _animator.Animator.Play(str, FACE_LAYER_INDEX);
            _animator.Animator.SetLayerWeight(FACE_LAYER_INDEX, 1f);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                _faceStateNames = AnimationEditorUtility.FindStateNames(animator, FACE_LAYER_INDEX);

                if (_faceStateNames != null && _faceStateNames.Count > 0)
                {
                    _faceStateNames.Sort();
                    _defaultFaceAnimationIndex = _faceStateNames.FindIndex(stateName => stateName.Contains("default"));
                    if (_defaultFaceAnimationIndex == -1) _defaultFaceAnimationIndex = 0;
                }
            }
#endif
        }
    }
}