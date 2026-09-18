using Unity.Netcode;
using UnityEngine;
using PlayerAPI;
using PlayerNetcode;
using Unity.Netcode.Components;

namespace PlayerNetcode
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] internal PlayerSettingSO SettingSO;
        [SerializeField] internal PlayerInput PlayerInput;
        [SerializeField] internal PlayerAnimation PlayerAnimation;
        [SerializeField] internal PlayerLocomotion PlayerLocomotion;
        [SerializeField] internal PlayerCamera PlayerCamera;
        [SerializeField] internal PlayerVisual PlayerVisual;
        [SerializeField] internal PlayerServer PlayerServer;
        [SerializeField] internal PlayerClient PlayerClient;
        [SerializeField] private NetworkTransform _netTransform;

        internal readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        internal PlayerEvent Event = new();

        protected override void OnNetworkPostSpawn()
        {
            PlayerServer.enabled = IsServer;
            PlayerClient.enabled = IsClient;

            _netTransform.enabled = !(IsOwner && !IsServer);
        }
    }
}