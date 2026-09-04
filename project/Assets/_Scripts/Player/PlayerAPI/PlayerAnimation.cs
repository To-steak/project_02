using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Assertions;
using UnityChan;
using Unity.Netcode;

namespace PlayerAPI
{
    public class PlayerAnimation : MonoBehaviour, IAnimationEventReceiver
    {
        NetworkAnimator _animator;
        PlayerEvent _event;

        readonly int Speed = Animator.StringToHash("Speed");

        const float IDLE = 0f;
        const float WALK = 1f;
        const float RUN = 2f;
        const int FACE_LAYER_INDEX = 1;

        [HideInInspector][SerializeField] private List<string> m_faceStateNames = new List<string>();
        [HideInInspector][SerializeField] private int m_defaultFaceAnimationIndex = 0;
        private HashSet<string> m_faceStateNameSet;

        public void Initialize(PlayerEvent playerEvent)
        {
            _event = playerEvent;
            _animator = GetComponent<NetworkAnimator>();
            m_faceStateNameSet = new HashSet<string>(m_faceStateNames);
        }

        public void PlayIdle()
        {
            _animator.Animator.SetFloat(Speed, IDLE);
        }

        public void PlayWalk()
        {
            _animator.Animator.SetFloat(Speed, WALK);
        }

        public void PlayRun()
        {
            _animator.Animator.SetFloat(Speed, RUN);
        }

        public void PlayJump()
        {
            _animator.SetTrigger("Jump");
        }

        public void NotifyAnimationCallback() => _event.RaiseAnimationCallback();
        public void NotifyAnimationCommit() => _event.RaiseAnimationCommit();

        private void OnCallChangeFace(string str)
        {
            str = str.Split('@')[0];
            Assert.IsNotNull(m_faceStateNameSet);
            if (m_faceStateNameSet.Contains(str))
            {
                TryOverrideFaceAnimation(str);
            }
            else
            {
                Assert.IsTrue(m_faceStateNames.Count > 0, "No face animation states found in the animator controller.");
                if (m_faceStateNames.Count > 0)
                {
                    TryOverrideFaceAnimation(m_faceStateNames[m_defaultFaceAnimationIndex]);
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
                m_faceStateNames = AnimationEditorUtility.FindStateNames(animator, FACE_LAYER_INDEX);

                if (m_faceStateNames != null && m_faceStateNames.Count > 0)
                {
                    m_faceStateNames.Sort();
                    m_defaultFaceAnimationIndex = m_faceStateNames.FindIndex(stateName => stateName.Contains("default"));
                    if (m_defaultFaceAnimationIndex == -1) m_defaultFaceAnimationIndex = 0;
                }
            }
#endif
        }
    }
}