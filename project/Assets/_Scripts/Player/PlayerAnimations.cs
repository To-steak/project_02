using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    Animator animator;

    readonly int speed = Animator.StringToHash("speed");

    public void Initialize()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        animator.SetFloat(speed, 0f);
    }

    public void PlayWalk()
    {
        animator.SetFloat(speed, 1f);
    }

    public void PlayRun()
    {
        animator.SetFloat(speed, 2f);
    }
}
