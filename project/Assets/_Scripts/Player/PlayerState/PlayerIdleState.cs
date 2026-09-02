using UnityEngine;
using PlayerState;

namespace PlayerState
{
    public class PlayerIdleState : BaseState
    {
        public PlayerIdleState(PlayerController controller) : base(controller)
        {
            CanMove = true;
            MoveSpeed = 0f;
        }

        public override void Enter()
        {
            controller.Animation.PlayIdle();
        }

        public override void Tick()
        {
            if (controller.Input.Move != Vector3.zero)
            {
                server.ChangeState(controller.Walk);
                return;
            }
        }

        public override void Exit()
        {

        }

        public override void OnJump()
        {
            if (controller.Locomotion.IsGrounded)
            {
                server.ChangeState(controller.Jump);
            }
        }
    }
}