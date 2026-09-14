using Unity.Netcode;
using UnityEngine;
using PlayerAPI;
using PlayerNetcode;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    public PlayerSettingSO SettingSO;

    [HideInInspector] public PlayerInput Input;
    [HideInInspector] public PlayerAnimation Animation;
    [HideInInspector] public PlayerLocomotion Locomotion;
    [HideInInspector] public PlayerCamera Camera;
    [HideInInspector] public PlayerVisual Visual;
    public PlayerEvent Event;

    [HideInInspector] public PlayerServer Server;
    [HideInInspector] public PlayerClient Client;
    [HideInInspector] public NetworkTransform NetTransform;

    public readonly NetworkVariable<float> AimPitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        Input = GetComponent<PlayerInput>();
        Animation = GetComponentInChildren<PlayerAnimation>();
        Locomotion = GetComponent<PlayerLocomotion>();
        Camera = GetComponent<PlayerCamera>();
        Visual = GetComponent<PlayerVisual>();
        Event = new PlayerEvent();

        Server = GetComponent<PlayerServer>();
        Client = GetComponent<PlayerClient>();
        NetTransform = GetComponent<NetworkTransform>();
    }

    public override void OnNetworkSpawn()
    {
        Input.Initialize(Event);
        Animation.Initialize(Event);
        Locomotion.Initialize();
    }

    protected override void OnNetworkPostSpawn()
    {
        Server.enabled = IsServer;
        Client.enabled = IsClient;
        NetTransform.enabled = !(IsOwner && !IsServer);

        NetworkAnimator animator = Animation.GetComponent<NetworkAnimator>();
        Debug.Log($"[{NetworkManager.Singleton.LocalClientId}] obj:{NetworkObjectId} " + $"server:{Server.NetworkBehaviourId} client:{Client.NetworkBehaviourId} " + $"anim:{(animator != null ? animator.NetworkBehaviourId.ToString() : "null")}");
    }

    public void Simulate(InputPayload payload)
    {
        Input.Apply(payload);

        Locomotion.CheckGrounded(SettingSO.GroundCheckRadius, SettingSO.GroundLayer);
        Locomotion.ApplyGravity(SettingSO.GravityValue);
        
        float speed = payload.Move == Vector3.zero ? 0f : (payload.Run ? SettingSO.RunSpeed : SettingSO.WalkSpeed);
        Locomotion.Move(payload.Move, speed, payload.Yaw);
    }

    public void ApplyPitch(float pitch)
    {
        AimPitch.Value = Mathf.Clamp(pitch, SettingSO.MinPitch, SettingSO.MaxPitch);
    }
}