using UnityEngine;

namespace Project02.Combat
{
    // 큐 항목: 예측 피격 연출. ShotPredictSystem이 world.PredictedImpacts에 적고, ImpactViewSystem이 LateUpdate에 비운다
    public readonly struct ImpactInfo
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly ImpactKind Kind;

        public ImpactInfo(Vector3 point, Vector3 normal, ImpactKind kind)
        {
            Point = point;
            Normal = normal;
            Kind = kind;
        }
    }
}
