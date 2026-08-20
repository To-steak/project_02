using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerSettings settings;

    PlayerState _state;
    public PlayerIdleState Idle;
    public PlayerWalkState Walk;

    public PlayerInputs Inputs;
    public PlayerAnimations Animations;
    public PlayerLocomotions Locomotions;

    void Awake()
    {
        if (settings == null)
        {
#if UNITY_EDITOR
            Debug.LogError("PlayerSettings is null in PlayerController.cs");
#endif
        }

        Inputs = GetComponent<PlayerInputs>();
        Animations = GetComponent<PlayerAnimations>();
        Locomotions = GetComponent<PlayerLocomotions>();

        Idle = new PlayerIdleState(this);
        Walk = new PlayerWalkState(this);
    }

    void OnEnable()
    {
        Inputs.Initialize();
        Animations.Initialize();
        Locomotions.Initialize(this, settings);

        Inputs.ActiveInputs();
    }

    void OnDisable()
    {
        Inputs.DeactiveInputs();
    }

    void Start()
    {
        _state = Idle;
    }

    void Update()
    {
        _state.Tick();
    }

    public void ChangeState(PlayerState state)
    {
        _state.Exit();
        _state = state;
        _state.Enter();
    }
}
