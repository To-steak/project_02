using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public PlayerSettingSO SettingSO;

    public PlayerIdleState Idle;
    public PlayerWalkState Walk;

    public PlayerInputs Inputs;
    public PlayerAnimations Animations;
    public PlayerLocomotions Locomotions;

    public PlayerServer Server;
    public PlayerClient Client;

    void Awake()
    {
        if (SettingSO == null)
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

        Server = GetComponent<PlayerServer>();
        Client = GetComponent<PlayerClient>();
    }

    public override void OnNetworkSpawn()
    {
        Inputs.Initialize();
        Animations.Initialize();
        Locomotions.Initialize();
    }

    protected override void OnNetworkPostSpawn()
    {
        Server.enabled = IsServer;
        Client.enabled = IsClient;
    }
}
