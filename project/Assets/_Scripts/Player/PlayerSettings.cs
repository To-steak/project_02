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
    [Header("Level")] public PlayerLevelTable LevelTable;
    [Header("MP")] 
    public float RunCost = 10.0f;
    public float JumpCost = 15.0f;
    public float RegenMp = 5.0f;
    [Range(0.0f, 1.0f)] public float RecoveryRatio = 1.0f; // MP 모두 소모하면 이 비율까지 회복해야 한다.
}
