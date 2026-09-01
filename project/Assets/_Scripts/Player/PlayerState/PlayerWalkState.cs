using UnityEngine;
using PlayerState;

public class PlayerWalkState : BaseState
{
    public PlayerWalkState(PlayerController controller) : base(controller)
    {
        CanMove = true;
        MoveSpeed = controller.SettingSO.WalkSpeed;
    }

    public override void Enter()
    {
        controller.Animation.PlayWalk();
    }

    public override void Tick()
    {
        if (controller.Input.Jump && controller.Locomotion.IsGrounded)
        {
            server.ChangeState(controller.Jump);
            return;
        }

        if (controller.Input.Move == Vector3.zero)
        {
            server.ChangeState(controller.Idle);
            return;
        }

        if (controller.Input.Run)
        {
            server.ChangeState(controller.Run);
            return;
        }
    }

    public override void Exit()
    {

    }
}
