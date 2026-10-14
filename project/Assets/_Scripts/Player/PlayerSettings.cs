using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpSpeed;

    [Header("Profile")]
    public CharacterProfile Profile;

    [Header("Mouse Input")]
    public float RotationSpeed;
    public float PitchSpeed;
    public float MinPitch;
    public float MaxPitch;

    [Header("Aim")]
    [Tooltip("조준점 레이가 맞는 레이어. 플레이어 자신의 레이어는 빼 둘 것")]
    public LayerMask AimLayer;
    [Tooltip("조준점을 찾는 최대 거리(m). 아무것도 맞지 않으면 이 거리의 지점을 쓴다")]
    public float AimDistance = 1000.0f;

    [Header("ADS Camera")]
    public float AdsCameraDistance = 1.25f;
    public float AdsVerticalArmLength = 0.0f;
    public Vector3 AdsShoulderOffset = new(0.6f, 0.0f, 0.0f);
    [Tooltip("조준 전환 속도(초당 0→1)")]
    public float AdsSpeed = 8.0f;

    [Header("Recoil")]
    [Tooltip("남은 반동이 카메라에 흡수되는 속도. 클수록 짧고 강하게 튄다")]
    public float RecoilSpeed = 20.0f;

    [Header("Visual")]
    [Tooltip("모델이 총구에서 조준점 방향으로 도는 속도(도/초)")]
    public float TurnSpeed = 720.0f;

    [Header("Server Validation")]
    [Tooltip("루트(발밑)에서 총구까지 허용하는 최대 거리(m). 넘으면 잘라낸다")]
    public float MaxMuzzleOffset = 2.0f;

    [Header("Level")]
    public PlayerLevelTable LevelTable;

    [Header("MP")]
    public float RunCost = 10.0f;
    public float JumpCost = 15.0f;
    public float RegenMp = 5.0f;
    [Range(0.0f, 1.0f)] public float RecoveryRatio = 1.0f; // MP 모두 소모하면 이 비율까지 회복해야 한다.
}