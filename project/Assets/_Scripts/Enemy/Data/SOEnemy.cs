using Project02.Character;
using UnityEngine;

[CreateAssetMenu(fileName = "SOEnemy", menuName = "Scriptable Objects/SOEnemy")]
public class SOEnemy : ScriptableObject
{
    public CharacterProfile Profile;
    public float WalkSpeed;
    public float RunSpeed;
    public float RotationSpeed;
    public int RewardExp;
    public int RewardGold;
    public float PerceptionRange;
    public float BattleRange;
    public float AttackRange;
}
