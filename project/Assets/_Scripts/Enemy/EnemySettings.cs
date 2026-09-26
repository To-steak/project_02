using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Scriptable Objects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    [Header("Movement")]
    public float WalkSpeed = 1.0f;
    public float RunSpeed = 3.0f;
    public float RotationSpeed = 360.0f;
    [Header("Profile")]
    public AgentProfile Profile;
}
