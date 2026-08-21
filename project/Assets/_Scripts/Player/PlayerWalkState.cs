using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController controller) : base(controller) { }

    public override PlayerStateType Type => PlayerStateType.Walk;
    public override bool CanMove => true;
    public override float MoveSpeed => controller.SettingSO.WalkSpeed;
    
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
