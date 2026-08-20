using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Enter: Idle");
        Animations.PlayIdle();
    }

    public override void Tick()
    {
        if (Inputs.Move != Vector3.zero)
        {
            controller.ChangeState(Walk);
            return;
        }

        Debug.Log("Current: Idle");
    }

    public override void Exit()
    {
        Debug.Log("Exit: Idle");
    }
}
