using UnityEngine;
using Project02.Weapon;

namespace Project02.Player
{
    // 플레이어 설정의 정본. Authoring·Proxy는 이 에셋 하나만 참조하고, 하위 설정은 여기서 꺼낸다
    [CreateAssetMenu(fileName = "SOPlayer", menuName = "Scriptable Objects/SOPlayer")]
    public class SOPlayer : ScriptableObject
    {
        [Header("Data")]
        public SOMovement Movement;
        public SOLevelTable LevelTable;
        public SOCatalog Catalog;

        [Header("Camera")]
        public float RotationSpeed;
        public float PitchSpeed;
        public float MinPitch;
        public float MaxPitch;
        public LayerMask AimLayer;
        public float AimDistance = 1000.0f;
        public float AdsCameraDistance = 1.25f;
        public float AdsVerticalArmLength = 0.0f;
        public Vector3 AdsShoulderOffset = new(0.6f, 0.0f, 0.0f);
        public float AdsSpeed = 8.0f;
        public float RecoilSpeed = 20.0f;

        [Header("Body")]
        public float TurnSpeed = 720.0f;
        public float MaxMuzzleOffset = 2.0f;
        public Vector3 FireOriginOffset = new(0.0f, 0.74f, 0.0f);
    }
}