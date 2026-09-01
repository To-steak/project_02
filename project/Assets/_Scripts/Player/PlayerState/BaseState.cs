using PlayerNetcode;

namespace PlayerState
{
    public abstract class BaseState
    {
        protected PlayerController controller;
        protected PlayerServer server;

        public bool CanMove { get; protected set; }
        public float MoveSpeed { get; protected set; }

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
    }
}