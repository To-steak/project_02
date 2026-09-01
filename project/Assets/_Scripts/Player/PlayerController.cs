using Unity.Netcode;
using UnityEngine;
using PlayerAPI;
using PlayerNetcode;
using PlayerState;

public class PlayerController : NetworkBehaviour
{
    public PlayerSettingSO SettingSO;

    public PlayerIdleState Idle;
    public PlayerWalkState Walk;
    public PlayerRunState Run;
    public PlayerJumpState Jump;

    public PlayerInput Input;
    public PlayerAnimation Animation;
    public PlayerLocomotion Locomotion;
    public PlayerCamera Camera;
    public PlayerEvent Event;

    public PlayerServer Server;
    public PlayerClient Client;

    public readonly NetworkVariable<float> AimPitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        if (SettingSO == null)
        {
#if UNITY_EDITOR
            Debug.LogError("PlayerSettings is null in PlayerController.cs");
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        Input = GetComponent<PlayerInput>();
        Animation = GetComponent<PlayerAnimation>();
        Locomotion = GetComponent<PlayerLocomotion>();
        Camera = GetComponent<PlayerCamera>();
        Event = new PlayerEvent();

        Idle = new PlayerIdleState(this);
        Walk = new PlayerWalkState(this);
        Run = new PlayerRunState(this);
        Jump = new PlayerJumpState(this);

        Server = GetComponent<PlayerServer>();
        Client = GetComponent<PlayerClient>();
    }

    public override void OnNetworkSpawn()
    {
        Input.Initialize();
        Animation.Initialize(Event);
        Locomotion.Initialize();
    }

    protected override void OnNetworkPostSpawn()
    {
        Server.enabled = IsServer;
        Client.enabled = IsClient;
    }
}