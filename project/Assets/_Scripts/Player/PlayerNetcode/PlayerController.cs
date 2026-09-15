using Unity.Netcode;
using UnityEngine;
using PlayerAPI;
using PlayerNetcode;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] internal PlayerSettingSO SettingSO;
    internal PlayerInput Input;
    internal PlayerAnimation Animation;
    internal PlayerLocomotion Locomotion;
    internal PlayerCamera Camera;
    internal PlayerVisual Visual;
    internal PlayerEvent Event;
    internal PlayerServer Server;
    internal PlayerClient Client;
    internal readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkTransform NetTransform;

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
        Input.Initialize();
        Animation.Initialize(Event);
        Locomotion.Initialize();
    }

    protected override void OnNetworkPostSpawn()
    {
        Server.enabled = IsServer;
        Client.enabled = IsClient;
        NetTransform.enabled = !(IsOwner && !IsServer);
    }

    public bool Simulate(InputPayload payload)
    {
        Locomotion.CheckGrounded(SettingSO.GroundCheckRadius, SettingSO.GroundLayer);

        bool jumped = payload.Jump && Locomotion.IsGrounded;
        if (jumped) Locomotion.Jump(SettingSO.JumpPower);

        Locomotion.ApplyGravity(SettingSO.GravityValue);

        float speed = payload.Move == Vector3.zero ? 0f : (payload.Run ? SettingSO.RunSpeed : SettingSO.WalkSpeed);
        Locomotion.Move(payload.Move, speed, payload.Yaw);

        return jumped;
    }

    public void ApplyPitch(float pitch)
    {
        Pitch.Value = Mathf.Clamp(pitch, SettingSO.MinPitch, SettingSO.MaxPitch);
    }
}