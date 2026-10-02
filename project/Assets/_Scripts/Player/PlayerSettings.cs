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
}
