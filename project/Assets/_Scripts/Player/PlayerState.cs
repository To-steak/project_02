public abstract class PlayerState
{
    protected PlayerController controller;
    protected PlayerInputs Inputs => controller.Inputs;
    protected PlayerAnimations Animations => controller.Animations;
    protected PlayerLocomotions Movements => controller.Locomotions;

    protected PlayerIdleState Idle => controller.Idle;
    protected PlayerWalkState Walk => controller.Walk;

    public PlayerState(PlayerController controller)
    {
        this.controller = controller;
    }

    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
}
