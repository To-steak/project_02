using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }

    public override PlayerStateType Type => PlayerStateType.Idle;
    public override bool CanMove => true;
    public override float MoveSpeed => 0f;

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
}
