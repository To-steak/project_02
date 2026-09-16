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
    internal readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
        Animation.Initialize(Event);
        Locomotion.Initialize();
    }

    protected override void OnNetworkPostSpawn()
    {
        Server.enabled = IsServer;
        Client.enabled = IsClient;
        NetTransform.enabled = !(IsOwner && !IsServer);
    }
}