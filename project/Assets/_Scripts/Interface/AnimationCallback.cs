using UnityEngine;

public class AnimationCallback : StateMachineBehaviour
{
    [Range(0, 1)] public float finishTime;

    private IAnimationEventReceiver _receiver;
    private bool _isTriggered;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isTriggered = false;

        if (_receiver == null)
        {
            _receiver = animator.GetComponent<IAnimationEventReceiver>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isTriggered && stateInfo.normalizedTime >= finishTime)
        {
            _isTriggered = true;
            _receiver?.NotifyAnimationCallback();
        }
    }
}