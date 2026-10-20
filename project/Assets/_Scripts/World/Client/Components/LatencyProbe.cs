#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;

namespace Project02.World
{
    // 디버그 Singleton Component: 입력 → 위치 변화 지연 측정
    // 정의: 이동 입력이 시작된 프레임 → 시뮬레이션 위치가 수평으로 1cm 이상 바뀐 프레임 (9월 측정과 같음)
    public sealed class LatencyProbe
    {
        public bool PredictionEnabled = true;
        public bool WasMoving;
        public float StartTime = -1.0f;
        public Vector3 StartPosition;
        public readonly List<float> Samples = new();   // ms
        public Vector3 LastPosition;
        public float StillSince;
        public int Rejected;
    }
}
#endif