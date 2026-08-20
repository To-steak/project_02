using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController controller) : base(controller) { }

    public override PlayerStateType Type => PlayerStateType.Walk;

    public override void Enter()
    {
        Debug.Log("Enter: Walk");
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
