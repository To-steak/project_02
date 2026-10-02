using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Collections;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] internal PlayerSettings PlayerSettings;
    [SerializeField] internal PlayerInput PlayerInput;
    [SerializeField] internal PlayerAnimation PlayerAnimation;
    [SerializeField] internal PlayerLocomotion PlayerLocomotion;
    [SerializeField] internal PlayerCamera PlayerCamera;
    [SerializeField] internal PlayerVisual PlayerVisual;
    [SerializeField] internal PlayerServer PlayerServer;
    [SerializeField] internal PlayerClient PlayerClient;
    [SerializeField] internal PlayerWeapon PlayerWeapon;
    [SerializeField] private NetworkTransform _networkTransform;

    internal readonly NetworkVariable<float> Pitch = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    internal readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    internal readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    internal PlayerEvent Event = new();

    public override void OnNetworkSpawn()
    {
        EquippedWeapon.OnValueChanged += (_, id) => PlayerWeapon.Equip(id, IsClient);
        PlayerWeapon.Equip(EquippedWeapon.Value, IsClient);

        if (IsServer)
        {
            EquippedWeapon.Value = 0;
            Nickname.Value = new FixedString64Bytes(GameServices.Session?.GetNickname(OwnerClientId) ?? $"Player#{OwnerClientId}");
        }
    }

    protected override void OnNetworkPostSpawn()
    {
        Debug.Assert(!IsHost, $"{name}: 호스트 모드는 지원하지 않음. 클라이언트 예측과 서버 판정이 함께 돌아 이중으로 이동한다.", this);
        PlayerServer.enabled = IsServer;
        PlayerClient.enabled = IsClient;

        _networkTransform.enabled = !(IsOwner && !IsServer);
    }

    public void Simulate(in InputPayload input)
    {
        PlayerLocomotion.Rotate(input.YawInput);
        PlayerLocomotion.Simulate(input, PlayerSettings);
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