using UnityEngine;
using Project02.Core;

namespace Project02.World
{
    // 유틸(순수): viewTick이 받은 스냅샷보다 앞설 때 쓸 마지막 두 스냅샷 찾기 (SnapshotUtil의 짝).
    // 위치는 호출하는 쪽이 Vector3.LerpUnclamped(prev, last, t)로 구한다: t > 1이면 두 스냅샷의 위치 차이(속도)로 앞으로 민다
    public static class ExtrapolationUtil
    {
        // viewTick 뒤쪽 스냅샷이 이미 있으면 false (보간으로 충분 → SnapshotUtil.TrySample). 두 개가 없어도 false.
        // 마지막 스냅샷보다 MAX_EXTRAPOLATION_TICKS 넘게 앞서면 거기서 멈춘다 (스냅샷이 끊겼을 때 끝없이 미끄러지지 않게)
        public static bool TryGetLastTwo<T>(TickBuffer<T> buffer, float viewTick, out T prev, out T last, out float t) where T : struct
        {
            int standard = Mathf.FloorToInt(viewTick);
            int range = buffer.Capacity / 2;
            prev = default;
            last = default;
            t = 0.0f;

            for (int j = 1; j < range; j++)
            {
                if (buffer.TryGetValue(standard + j, out _)) return false;
            }

            int lastTick = -1;
            for (int i = 0; i < range; i++)
            {
                int candidate = standard - i;
                if (buffer.TryGetValue(candidate, out last))
                {
                    lastTick = candidate;
                    break;
                }
            }
            if (lastTick < 0) return false;

            int prevTick = -1;
            for (int i = 1; i < range; i++)
            {
                int candidate = lastTick - i;
                if (buffer.TryGetValue(candidate, out prev))
                {
                    prevTick = candidate;
                    break;
                }
            }
            if (prevTick < 0) return false;

            float target = Mathf.Min(viewTick, lastTick + LagUtil.MAX_EXTRAPOLATION_TICKS);
            t = (target - prevTick) / (lastTick - prevTick);
            return true;
        }
    }
}