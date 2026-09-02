using PlayerNetcode;

namespace PlayerState
{
    public abstract class BaseState
    {
        protected PlayerController controller;
        protected PlayerServer server;

        public BaseState(PlayerController controller)
        {
            this.controller = controller;
            server = controller.GetComponent<PlayerServer>();
        }

        public abstract void Enter();
        public abstract void Tick();
        public abstract void Exit();

        public virtual void OnAnimationCallback() { }
        public virtual void OnAnimationCommit() { }
        public virtual void OnJump()
        {
            if (controller.Locomotion.IsGrounded)
            {
                server.ChangeState(controller.Jump);
            }
        }
    }
}