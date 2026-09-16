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
    }
}