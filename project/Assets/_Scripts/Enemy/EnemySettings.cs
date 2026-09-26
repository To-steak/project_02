using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Scriptable Objects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    [Header("Locomotion")]
    public float WalkSpeed = 1.0f;
    public float RunSpeed = 3.0f;
    public float RotationSpeed = 360.0f;
    public AgentProfile Profile;
}
