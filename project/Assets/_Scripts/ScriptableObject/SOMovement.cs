using UnityEngine;

namespace Project02.Player
{
    [CreateAssetMenu(fileName = "SOMovement", menuName = "Scriptable Objects/SOMovement")]
    public class SOMovement : ScriptableObject
    {
        public CharacterProfile Profile;

        public float WalkSpeed;
        public float RunSpeed;
        public float JumpSpeed;

        public float RunCost;
        public float JumpCost;

        public float RegenEnergy;
        public float RecoveryRatio;
    }
}