using UnityEngine;
using Project02.Character;

namespace Project02.Player
{
    [CreateAssetMenu(fileName = "SOMovement", menuName = "Scriptable Objects/SOMovement")]
    public class SOMovement : ScriptableObject
    {
        [Header("Profile")]
        public CharacterProfile Profile;

        [Header("Speed")]
        public float WalkSpeed;
        public float RunSpeed;
        public float JumpSpeed;

        [Header("Cost")]
        public float RunCost;
        public float JumpCost;
        
        [Header("Resource")]
        public float RegenEnergy;
        public float RecoveryRatio;
    }
}