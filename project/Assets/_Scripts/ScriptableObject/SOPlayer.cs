using UnityEngine;

namespace Project02.Player
{
    [CreateAssetMenu(fileName = "SOPlayer", menuName = "Scriptable Objects/SOPlayer")]
    public class SOPlayer : ScriptableObject
    {
        public CharacterProfile Profile;
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
        public float TurnSpeed = 720.0f;
        public float MaxMuzzleOffset = 2.0f;
        public PlayerLevelTable LevelTable;
    }
}