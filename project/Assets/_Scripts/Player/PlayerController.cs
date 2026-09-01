using Unity.Netcode;
using UnityEngine;
using PlayerAPI;
using PlayerNetcode;

public class PlayerController : NetworkBehaviour
{
    public PlayerSettingSO SettingSO;

    public PlayerIdleState Idle;
    public PlayerWalkState Walk;

    public PlayerInput Input;
    public PlayerAnimation Animation;
    public PlayerLocomotion Locomotion;
    public PlayerCamera Camera;

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

        Input = GetComponent<PlayerInput>();
        Animation = GetComponent<PlayerAnimation>();
        Locomotion = GetComponent<PlayerLocomotion>();
        Camera = GetComponent<PlayerCamera>();

        Idle = new PlayerIdleState(this);
        Walk = new PlayerWalkState(this);

        Server = GetComponent<PlayerServer>();
        Client = GetComponent<PlayerClient>();
    }

    public override void OnNetworkSpawn()
    {
        Input.Initialize();
        Animation.Initialize();
        Locomotion.Initialize();
    }

    protected override void OnNetworkPostSpawn()
    {
        Server.enabled = IsServer;
        Client.enabled = IsClient;
    }
}