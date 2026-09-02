using System;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerEvent
    {
        public event Action OnAnimationCallback;
        public event Action OnAnimationCommit;

        public void RaiseAnimationCallback() => OnAnimationCallback?.Invoke();
        public void RaiseAnimationCommit() => OnAnimationCommit?.Invoke();

        public event Action OnJump;
        public void RaiseJump() => OnJump?.Invoke();
    }
}