using Unity.Netcode.Components;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerAnimation : MonoBehaviour, IAnimationEventReceiver
    {
        NetworkAnimator _animator;
        PlayerEvent _event;

        readonly int Speed = Animator.StringToHash("Speed");
        readonly int Jump = Animator.StringToHash("Jump");

        const float IDLE = 0f;
        const float WALK = 1f;
        const float RUN = 2f;

        public void Initialize(PlayerEvent playerEvent)
        {
            _animator = GetComponent<NetworkAnimator>();
            _event = playerEvent;
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
    }
}