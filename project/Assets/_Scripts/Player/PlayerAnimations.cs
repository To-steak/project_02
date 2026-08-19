using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Init()
    { }
}
