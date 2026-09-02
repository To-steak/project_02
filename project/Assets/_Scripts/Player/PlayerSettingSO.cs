using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettingSO : ScriptableObject
{
    [Header("Movement")]
    public float WalkSpeed;
    public float RunSpeed;
    
    [Header("Physics")]
    public LayerMask HitLayer;
    public float GravityValue;
    public float JumpPower;

    [Header("Ground Check")]
    public float GroundCheckRadius;
    public LayerMask GroundLayer;

    [Header("Mouse Input")]
    public float RotationSpeed;
    public float PitchSpeed;
    public float MinPitch;
    public float MaxPitch;
}
