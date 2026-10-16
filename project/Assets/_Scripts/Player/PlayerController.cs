using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Project02.Combat;

namespace Project02.Player
{
    // Authoring: 형제 컴포넌트 참조 + 동기화 변수.
    // 시뮬레이션 사본은 PlayerServer / PlayerOwner가 소유
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] public SOPlayer PlayerSettings;
        [SerializeField] public PlayerAnimation PlayerAnimation;
        [SerializeField] public PlayerLocomotion PlayerLocomotion;
        [SerializeField] public PlayerCamera PlayerCamera;
        [SerializeField] public PlayerVisual PlayerVisual;
        [SerializeField] public PlayerServer PlayerServer;
        [SerializeField] public PlayerOwner PlayerOwner;
        [SerializeField] public PlayerRemote PlayerRemote;
        [SerializeField] public PlayerWeapon PlayerWeapon;
        [SerializeField] public PlayerProgression PlayerProgression;
        [SerializeField] public Health Health;

        public readonly NetworkVariable<int> EquippedWeapon = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public readonly NetworkVariable<FixedString64Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                EquippedWeapon.Value = 0;   // 기본 무기. 닉네임은 SessionSystem.Join이 넣는다
            }

            EquippedWeapon.OnValueChanged += OnEquippedWeaponChanged;
            PlayerWeapon.Equip(EquippedWeapon.Value, IsClient);   // 연출(모델·IK)만
        }

        public override void OnNetworkDespawn()
        {
            EquippedWeapon.OnValueChanged -= OnEquippedWeaponChanged;
        }

        // 연출만 (규칙 8): 시뮬레이션 무기는 EquipSystem → StatePayload → Restore
        private void OnEquippedWeaponChanged(int previous, int current)
        {
            PlayerWeapon.Equip(current, IsClient);
        }
    }
}