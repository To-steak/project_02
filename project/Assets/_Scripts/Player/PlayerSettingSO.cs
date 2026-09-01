using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettingSO : ScriptableObject
{
    [Header("Movement")]
    public float WalkSpeed;
    public float RunSpeed;
    
    [Header("Physics")]
    public float GravityValue;
    public float JumpHeight;

    [Header("Ground Check")]
    public float GroundCheckRadius;
    public LayerMask GroundLayer;

    [Header("Input")]
    public float RotationSpeed;
}
