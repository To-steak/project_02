using UnityEngine;

namespace Project02.Combat
{
    public static class RewindUtil
    {
        public static bool TrySample(RewindTarget target, float tick, out Vector3 position)
        {
            int from = Mathf.FloorToInt(tick);

            // 그 시점에 없었음 (그 뒤에 스폰) → 쏜 사람 화면에도 없었으니 맞을 수 없다
            if (!target.History.TryGetValue(from, out Vector3 a))
            {
                position = default;
                return false;
            }

            // 서버는 매 틱 기록하니 다음 틱이 없으면 a가 가장 최근
            if (!target.History.TryGetValue(from + 1, out Vector3 b))
            {
                position = a;
                return true;
            }

            position = Vector3.Lerp(a, b, tick - from);
            return true;
        }
    }
}