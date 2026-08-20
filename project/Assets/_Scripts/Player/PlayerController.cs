using Unity.Netcode;
using UnityEngine;

public enum PlayerStateType : byte
{
    Idle,
    Walk
}

public class PlayerController : NetworkBehaviour
{
    [SerializeField] PlayerSettings settings;

    PlayerState _state;
    public PlayerIdleState Idle;
    public PlayerWalkState Walk;

    public PlayerInputs Inputs;
    public PlayerAnimations Animations;
    public PlayerLocomotions Locomotions;

    NetworkVariable<PlayerStateType> _network;

    void Awake()
    {
        if (settings == null)
        {
#if UNITY_EDITOR
            Debug.LogError("PlayerSettings is null in PlayerController.cs");
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        Inputs = GetComponent<PlayerInputs>();
        Animations = GetComponent<PlayerAnimations>();
        Locomotions = GetComponent<PlayerLocomotions>();

        Idle = new PlayerIdleState(this);
        Walk = new PlayerWalkState(this);

        _network = new NetworkVariable<PlayerStateType>(PlayerStateType.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        Inputs.Initialize();
        Animations.Initialize();
        Locomotions.Initialize(this, settings);

        if (IsOwner)
        {
            Inputs.ActiveInputs();
        }

        _network.OnValueChanged += OnNetworkStateChanged;
        
        if (IsServer)
        {
            _state = Idle;
        }
    }

    public override void OnNetworkDespawn()
    {
        _network.OnValueChanged -= OnNetworkStateChanged;

        if (IsOwner)
        {
            Inputs.InactiveInputs();
        }
    }

    void Update()
    {
        if (IsServer)
        {
            _state?.Tick(); // OnNetworkSpawn > Update 의 실행순서를 보장하지 않는다.
        }

        if (IsOwner)
        {
            MoveRPC(Inputs.Move);
        }
    }

    public void ChangeState(PlayerState state)
    {
        _state.Exit();
        _state = state;
        _network.Value = _state.Type;
        _state.Enter();
    }

    private void OnNetworkStateChanged(PlayerStateType previous, PlayerStateType current)
    {
        switch (current)
        {
            case PlayerStateType.Idle:
                Animations.PlayIdle();
                break;
            case PlayerStateType.Walk:
                Animations.PlayWalk();
                break;
            default:
                break;
        }
    }

    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector3 move)
    {
        Inputs.SetMove(move);
    }
}
