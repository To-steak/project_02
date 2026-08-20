using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }

    public override PlayerStateType Type => PlayerStateType.Idle;

    public override void Enter()
    {
        Debug.Log("Enter: Idle");
    }

    public override void Tick()
    {
        if (Inputs.Move != Vector3.zero)
        {
            server.ChangeState(Walk);
            return;
        }

        Debug.Log("Current: Idle");
    }

    public override void Exit()
    {
        Debug.Log("Exit: Idle");
    }
}
