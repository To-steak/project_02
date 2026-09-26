using Unity.Netcode;
using UnityEngine;
using PlayerAPI;
using PlayerNetcode;
using Unity.Netcode.Components;

namespace PlayerNetcode
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] internal PlayerSettingSO PlayerSettings;
        [SerializeField] internal PlayerInput PlayerInput;
        [SerializeField] internal PlayerAnimation PlayerAnimation;
        [SerializeField] internal PlayerLocomotion PlayerLocomotion;
        [SerializeField] internal PlayerCamera PlayerCamera;
        [SerializeField] internal PlayerVisual PlayerVisual;
        [SerializeField] internal PlayerServer PlayerServer;
        [SerializeField] internal PlayerClient PlayerClient;
        [SerializeField] private NetworkTransform _networkTransform;

        internal readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        internal PlayerEvent Event = new();

        protected override void OnNetworkPostSpawn()
        {
            PlayerServer.enabled = IsServer;
            PlayerClient.enabled = IsClient;

            _networkTransform.enabled = !(IsOwner && !IsServer);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (PlayerSettings != null)
            {
                DrawCapsule();
            }
        }

        private void DrawCapsule()
        {
            float radius = PlayerSettings.Profile.Radius;
            Vector3 position = transform.position;
            LayerMask layer = PlayerSettings.Profile.GroundLayer | PlayerSettings.Profile.ObstacleLayer;

            CharacterPhysics.GetCapsule(position, radius, PlayerSettings.Profile.Height, out Vector3 bottom, out Vector3 top);

            bool overlapped = Physics.CheckCapsule(bottom, top, radius, layer, QueryTriggerInteraction.Ignore);
            Gizmos.color = overlapped ? Color.red : Color.blue;

            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawWireSphere(top, radius);

            Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
            Gizmos.DrawLine(bottom + Vector3.left * radius, top + Vector3.left * radius);
            Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
            Gizmos.DrawLine(bottom + Vector3.back * radius, top + Vector3.back * radius);

            // 올라설 수 있는 최대 턱 높이
            Vector3 step = position + Vector3.up * PlayerSettings.Profile.StepHeight;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(step + Vector3.right * radius, step + Vector3.forward * radius);
            Gizmos.DrawLine(step + Vector3.forward * radius, step + Vector3.left * radius);
            Gizmos.DrawLine(step + Vector3.left * radius, step + Vector3.back * radius);
            Gizmos.DrawLine(step + Vector3.back * radius, step + Vector3.right * radius);

            if (Application.isPlaying)
            {
                Gizmos.color = PlayerLocomotion.State.IsGrounded ? Color.yellow : Color.gray;
                Gizmos.DrawLine(position, position + Vector3.down * (radius * 0.5f));
            }
        }
#endif
    }
}