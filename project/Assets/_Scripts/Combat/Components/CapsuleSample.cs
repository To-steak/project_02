using UnityEngine;

namespace Project02.Combat
{
    // 판정 시점에 샘플한 대상 캡슐 하나. 부르는 쪽이 채워 ProjectileStepUtil에 넘긴다
    // (서버: RewindUtil로 되감은 위치, 클라: SnapshotUtil로 보간한 위치)
    public readonly struct CapsuleSample
    {
        public readonly Vector3 Position;   // 발밑
        public readonly float Radius;
        public readonly float Height;
        public readonly int Index;          // 부르는 쪽 목록의 인덱스 (서버: world.Hittables, 클라: world.HitTargets)

        public CapsuleSample(Vector3 position, float radius, float height, int index)
        {
            Position = position;
            Radius = radius;
            Height = height;
            Index = index;
        }
    }
}
