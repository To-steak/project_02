using UnityEngine;

namespace Project02.Player
{
    public readonly struct RemoteFire
    {
        public readonly int Tick;
        public readonly Vector3 AimPoint;
        public readonly Vector2 Spread;

        public RemoteFire(int tick, Vector3 aimPoint, Vector2 spread)
        {
            Tick = tick;
            AimPoint = aimPoint;
            Spread = spread;
        }
    }
}