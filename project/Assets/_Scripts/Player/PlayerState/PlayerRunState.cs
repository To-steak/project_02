using UnityEngine;
using PlayerState;

public class PlayerRunState : BaseState
{
    public PlayerRunState(PlayerController controller) : base(controller)
    {
        CanMove = true;
        MoveSpeed = controller.SettingSO.RunSpeed;
    }

    public override void Enter()
    {
        controller.Animation.PlayRun();
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

        if (!controller.Input.Run)
        {
            server.ChangeState(controller.Walk);
            return;
        }
    }

    public override void Exit()
    {

    }
}