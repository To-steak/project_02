using Project02.Core;
using UnityEngine;

namespace Project02.Combat
{
    public sealed class RewindTarget
    {
        public const int CAPACITY = 64;   // 64틱 = 1.28초. MAX_LAG_TICKS(25)보다 넉넉히

        public readonly TickBuffer<Vector3> History = new(CAPACITY);   // 틱 끝의 발밑 위치 (스냅샷과 같은 시점)
        public float Radius;
        public float Height;
    }
}