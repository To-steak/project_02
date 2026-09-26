using UnityEngine;

[CreateAssetMenu(fileName = "AgentProfile", menuName = "Scriptable Objects/AgentProfile")]
public class AgentProfile : ScriptableObject
{
    public float Radius = 0.5f;
    public float Height = 1.0f;
    [Range(0f, 89f)] public float SlopeLimit = 45.0f;
    public float StepHeight = 0.2f;
    public float Gravity = 9.81f;
    public float MaxFallSpeed = 20.0f;
    public float GroundStickSpeed = 5.0f;
    public LayerMask GroundLayer;
    public LayerMask ObstacleLayer;
}
