using System;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerEvent
    {
        public event Action<AnimationID> OnAnimationCallback;
        public event Action<AnimationID> OnAnimationCommit;

        public void RaiseAnimationCallback(AnimationID id) => OnAnimationCallback?.Invoke(id);
        public void RaiseAnimationCommit(AnimationID id) => OnAnimationCommit?.Invoke(id);

        public event Action OnJump;
        public void RaiseJump() => OnJump?.Invoke();

        public event Action OnJumpExecute;
        public void RaiseJumpExecute() => OnJumpExecute?.Invoke();
    }
}