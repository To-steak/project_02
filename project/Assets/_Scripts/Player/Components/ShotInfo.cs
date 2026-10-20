using UnityEngine;

namespace Project02.Player
{
    public readonly struct ShotInfo
    {
        public readonly FireStance Stance;
        public readonly Vector2 Spread;

        public ShotInfo(FireStance stance, Vector2 spread)
        {
            Stance = stance;
            Spread = spread;
        }
    }
}