using UnityEngine;
using Project02.Combat;

namespace Project02.World
{
    // 유틸(순수): 클라의 예상 되감기 양과 외삽 초과분. 계산은 이 함수 하나에만 둔다 (호출은 ViewTickSystem 한 곳, 결과는 ClientWorld에)
    public static class LagUtil
    {
        public const float MAX_EXTRAPOLATION_TICKS = 10.0f;   // 200ms. 이보다 먼 미래는 외삽 오차가 커서 그리지 않는다 (서버 판정은 상한까지만 되감는다)

        // 예상 lag = (LatestTick − RenderTick) + RttTicks + SmoothedBuffer
        // 근거: 서버가 이 입력을 소비하는 틱 ≈ 지금 받은 최신 스냅샷 틱 + 왕복 시간 + 서버 입력 버퍼.
        //       그러니 (소비 틱 − RenderTick)이 서버가 되감아야 할 양이다.
        // 세 값 모두 "틱 수"(차이)라 서버 축과 클라 축을 직접 빼지 않는다.
        // 반환: 초과 = clamp(예상 lag − MAX_LAG_TICKS, 0, MAX_EXTRAPOLATION_TICKS). 스냅샷 전이거나 RTT 측정 전(RttTicks < 0)이면 0 (외삽하지 않음), expectedLag = -1
        public static float GetExtrapolation(in InterpolationClock interp, in ClientClock clock, out float expectedLag)
        {
            if (!interp.HasTick || clock.RttTicks < 0.0f)
            {
                expectedLag = -1.0f;
                return 0.0f;
            }

            expectedLag = (interp.LatestTick - interp.RenderTick) + clock.RttTicks + clock.SmoothedBuffer;
            return Mathf.Clamp(expectedLag - RewindTarget.MAX_LAG_TICKS, 0.0f, MAX_EXTRAPOLATION_TICKS);
        }
    }
}