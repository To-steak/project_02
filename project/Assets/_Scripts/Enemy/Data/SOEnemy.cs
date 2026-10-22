using UnityEngine;
using Project02.Character;

namespace Project02.Enemy
{
    [CreateAssetMenu(fileName = "SOEnemy", menuName = "Scriptable Objects/SOEnemy")]
    public class SOEnemy : ScriptableObject
    {
        public CharacterProfile Profile;
        public int MaxHp;   // 정본. EnemyServerAuthoring.OnNetworkSpawn이 Health에 채운다
        public float WalkSpeed;
        public float RunSpeed;
        public float RotationSpeed;
        public int RewardExp;
        public int RewardGold;
        public float PerceptionRange;
        public float BattleRange;
        public float AttackRange;
    }
}
