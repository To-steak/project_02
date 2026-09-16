using UnityEngine;

public class AnimationCallback : StateMachineBehaviour
{
    [SerializeField] private AnimationID _id;
    private IAnimationEventReceiver _receiver;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_receiver == null)
        {
            _receiver = animator.GetComponent<IAnimationEventReceiver>();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _receiver?.NotifyAnimationCallback(_id);
    }
}