using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettingSO : ScriptableObject
{
    [Header("Movement")]
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpPower;
    [Header("Profile")]
    public AgentProfile Profile;
    [Header("Mouse Input")]
    public float RotationSpeed;
    public float PitchSpeed;
    public float MinPitch;
    public float MaxPitch;
}
