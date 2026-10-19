#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;

namespace Project02.World
{
    // 디버그: 지연 측정과 예측 켜기/끄기. ClientLoop가 틱 루프와 transform 출력 뒤에 부른다
    public static class LatencySystem
    {
        private const float MOVE_THRESHOLD_SQR = 0.01f * 0.01f;   // 1cm

        public static void Measure(ClientWorld world, Vector3 position, float now)
        {
            LatencyProbe probe = world.Probe;

            bool moving = world.Input.Move != Vector2.zero;
            if (moving && !probe.WasMoving)
            {
                probe.StartTime = now;
                probe.StartPosition = position;
            }
            probe.WasMoving = moving;

            if (probe.StartTime < 0.0f) return;

            Vector3 delta = position - probe.StartPosition;
            delta.y = 0.0f;
            if (delta.sqrMagnitude > MOVE_THRESHOLD_SQR)
            {
                probe.Samples.Add((now - probe.StartTime) * 1000.0f);
                probe.StartTime = -1.0f;
            }
        }

        public static void Toggle(ClientWorld world)
        {
            LatencyProbe probe = world.Probe;
            probe.PredictionEnabled = !probe.PredictionEnabled;
            probe.Samples.Clear();
            probe.StartTime = -1.0f;
        }

        public static void Summarize(List<float> samples, out float median, out float p95, out float max)
        {
            median = p95 = max = 0.0f;
            int count = samples.Count;
            if (count == 0) return;

            float[] sorted = samples.ToArray();
            System.Array.Sort(sorted);
            median = sorted[count / 2];
            p95 = sorted[Mathf.Min(count - 1, Mathf.CeilToInt(count * 0.95f) - 1)];
            max = sorted[count - 1];
        }
    }
}
#endif