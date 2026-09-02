using UnityEngine;
using PlayerState;

namespace PlayerState
{
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
        
        public override void OnJump()
        {
            if (controller.Locomotion.IsGrounded)
            {
                server.ChangeState(controller.Jump);
            }
        }
    }
}