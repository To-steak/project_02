using UnityEngine;

namespace Project02.Combat
{
    // ProjectileStepUtil.Step이 Target/Wall일 때 채우는 값
    public readonly struct ProjectileHit
    {
        public readonly int Sample;        // Target: 넘긴 샘플 목록의 인덱스 (대상은 samples[Sample].Index). Wall: -1
        public readonly float Distance;    // 발사 지점부터 맞은 곳까지 날아간 거리 (거리 감쇠용)
        public readonly Vector3 Point;     // 맞은 지점
        public readonly Vector3 Normal;    // 벽: 표면 법선, 대상: 탄 진행 반대 방향

        public ProjectileHit(int sample, float distance, Vector3 point, Vector3 normal)
        {
            Sample = sample;
            Distance = distance;
            Point = point;
            Normal = normal;
        }
    }
}
