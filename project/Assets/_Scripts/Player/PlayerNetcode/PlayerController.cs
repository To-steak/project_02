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
        Pitch.Value = Mathf.Clamp(pitch, SettingSO.MinPitch, SettingSO.MaxPitch);
    }
}