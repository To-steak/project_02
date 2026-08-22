public enum PlayerStateType : byte
{
    Idle,
    Walk
}

public abstract class PlayerState
{
    protected PlayerController controller;
    protected PlayerServer server;

    protected PlayerInputs Inputs => controller.Inputs;
    protected PlayerAnimations Animations => controller.Animations;
    protected PlayerLocomotions Locomotions => controller.Locomotions;

    protected PlayerIdleState Idle => controller.Idle;
    protected PlayerWalkState Walk => controller.Walk;

    public PlayerStateType Type { get; protected set; }
    public bool CanMove { get; protected set; }
    public float MoveSpeed { get; protected set; }

    public PlayerState(PlayerController controller)
    {
        this.controller = controller;
        server = controller.GetComponent<PlayerServer>();
    }

    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
    public virtual void PlayAnimation() { }
}
