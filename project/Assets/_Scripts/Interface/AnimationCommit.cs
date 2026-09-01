using System;
using UnityEngine;

public class AnimationCommit : StateMachineBehaviour
{
    [Range(0, 1)] public float[] commitTimes;

    private bool[] _isTriggered;
    private IAnimationEventReceiver _receiver;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_receiver == null)
        {
            _receiver = animator.GetComponent<IAnimationEventReceiver>();
        }

        if (commitTimes != null)
        {
            if (_isTriggered == null || _isTriggered.Length != commitTimes.Length)
            {
                _isTriggered = new bool[commitTimes.Length];
            }
            else
            {
                Array.Clear(_isTriggered, 0, _isTriggered.Length);
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (commitTimes == null || _isTriggered == null)
        {
            return;
        }

        for (int i = 0; i < commitTimes.Length; i++)
        {

            if (!_isTriggered[i] && stateInfo.normalizedTime >= commitTimes[i])
            {
                _isTriggered[i] = true;
                _receiver?.NotifyAnimationCommit();
            }
        }
    }
}