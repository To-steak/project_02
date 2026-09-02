using UnityEngine;
using PlayerState;

namespace PlayerState
{
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

        public override void OnJump()
        {
            if (controller.Locomotion.IsGrounded)
            {
                server.ChangeState(controller.Jump);
            }
        }
    }

}