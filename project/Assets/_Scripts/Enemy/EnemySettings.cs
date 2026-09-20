using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Scriptable Objects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    [Header("Layer")]
    public LayerMask ObstacleLayer;
    public LayerMask GroundLayer;
    [Header("Locomotion")]
    public float WalkSpeed = 1.0f;
    public float RunSpeed = 3.0f;
    public float RotationSpeed = 360.0f;
    [Header("Collide")]
    public float Radius = 0.5f;
    public float Height = 1.0f;
    [Range(0.0f, 89.0f)] public float SlopeLimit = 45.0f;
    public float StepHeight = 0.2f;
    [Header("Gravity")]
    public float Gravity = -9.81f;
    public float MaxFallSpeed = 20.0f;
    public float GroundStickSpeed = -5.0f;
}
