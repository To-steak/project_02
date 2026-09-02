using PlayerState;
using UnityEngine;

namespace PlayerState
{
    public class PlayerJumpState : BaseState
    {
        public PlayerJumpState(PlayerController controller) : base(controller)
        {
            MoveSpeed = controller.SettingSO.WalkSpeed;
        }

        public override void Enter()
        {
            
        }

        public override void Tick()
        {

        }

        public override void Exit()
        {

        }

        public override void OnAnimationCommit()
        {
            controller.Locomotion.Jump(controller.SettingSO.JumpPower);
        }

        public override void OnAnimationCallback()
        {
            server.ChangeState(controller.Idle);
        }
    }
}