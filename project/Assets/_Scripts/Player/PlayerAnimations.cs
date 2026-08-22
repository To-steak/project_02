using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    NetworkAnimator _animator;

    readonly int speed = Animator.StringToHash("speed");

    public void Initialize()
    {
        _animator = GetComponent<NetworkAnimator>();
    }

    public void PlayIdle()
    {
        _animator.Animator.SetFloat(speed, 0f);
    }

    public void PlayWalk()
    {
        _animator.Animator.SetFloat(speed, 1f);
    }

    public void PlayRun()
    {
        _animator.Animator.SetFloat(speed, 2f);
    }
}
