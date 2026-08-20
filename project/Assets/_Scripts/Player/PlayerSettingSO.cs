using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettingSO : ScriptableObject
{
    public float WalkSpeed;
    public float RunSpeed;
    public float GravityValue;
    public float JumpHeight;
}
