using PlayerAPI;
using PlayerNetcode;

namespace PlayerState
{
    public abstract class BaseState
    {
        protected PlayerController controller;
        protected PlayerServer server;

        protected PlayerInput Inputs => controller.Input;
        protected PlayerAnimation Animations => controller.Animation;
        protected PlayerLocomotion Locomotions => controller.Locomotion;

        protected PlayerIdleState Idle => controller.Idle;
        protected PlayerWalkState Walk => controller.Walk;

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
    }
}