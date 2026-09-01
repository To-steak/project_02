using PlayerState;

namespace PlayerState
{
    public class PlayerJumpState : BaseState
    {
        public PlayerJumpState(PlayerController controller) : base(controller)
        {
            CanMove = true;
            MoveSpeed = controller.SettingSO.WalkSpeed;
        }

        public override void Enter()
        {
            controller.Animation.PlayJump();
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
            controller.Input.SetJump(false);
            server.ChangeState(controller.Idle);
        }
    }
}