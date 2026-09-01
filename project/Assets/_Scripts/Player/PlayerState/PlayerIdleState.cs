using UnityEngine;
using PlayerState;

public class PlayerIdleState : BaseState
{
    public PlayerIdleState(PlayerController controller) : base(controller)
    {
        CanMove = true;
        MoveSpeed = 0f;
    }

    public override void Enter()
    {
        controller.Animations.PlayIdle();
    }

    public override void Tick()
    {
        if (Inputs.Move != Vector3.zero)
        {
            server.ChangeState(Walk);
            return;
        }
    }

    public override void Exit()
    {
        
    }
}
