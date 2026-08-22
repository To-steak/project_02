using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController controller) : base(controller)
    {
        CanMove = true;
        MoveSpeed = 0f;
    }

    public override void Enter()
    {
        
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

    public override void PlayAnimation()
    {
        controller.Animations.PlayIdle();
    }
}
