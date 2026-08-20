using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Enter: Walk");
        Animations.PlayWalk();
    }

    public override void Tick()
    {
        if (Inputs.Move == Vector3.zero)
        {
            controller.ChangeState(Idle);
            return;
        }

        Debug.Log("Current: Walk");
    }

    public override void Exit()
    {
        Debug.Log("Exit: Walk");
    }
}
