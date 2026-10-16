using Project02.Core;
using UnityEngine;

namespace Project02.Client
{
    // 유틸(순수): RenderTick 앞뒤 스냅샷 찾기. 유실된 틱은 건너뛰고 가장 가까운 것을 쓴다
    public static class SnapshotUtil
    {
        public static bool TrySample<T>(TickBuffer<T> buffer, float renderTick, out T from, out T to, out float t) where T : struct
        {
            int standard = Mathf.FloorToInt(renderTick);
            int range = buffer.Capacity / 2;

            int fromTick = -1;
            from = default;
            for (int i = 0; i < range; i++)
            {
                int candidate = standard - i;
                if (buffer.TryGetValue(candidate, out from))
                {
                    fromTick = candidate;
                    break;
                }
            }

            if (fromTick < 0)
            {
                to = default;
                t = 0.0f;
                return false;
            }

            for (int j = 1; j < range; j++)
            {
                int candidate = standard + j;
                if (buffer.TryGetValue(candidate, out to))
                {
                    t = (renderTick - fromTick) / (candidate - fromTick);
                    return true;
                }
            }

            to = from;   // 뒤쪽이 아직 없음 → 마지막 값에 멈춤
            t = 0.0f;
            return true;
        }
    }
}