using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController controller) : base(controller)
    {
        Type = PlayerStateType.Walk;
        CanMove = true;
        MoveSpeed = controller.SettingSO.WalkSpeed;
    }

    public override void Enter()
    {

    }

    public override void Tick()
    {
        if (Inputs.Move == Vector3.zero)
        {
            server.ChangeState(Idle);
            return;
        }
    }

    public override void Exit()
    {

    }
}
