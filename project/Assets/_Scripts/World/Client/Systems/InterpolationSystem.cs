using UnityEngine;
using Project02.Core;

namespace Project02.World
{
    // 보간 시계만 관리. 무엇을 그릴지는 *ViewSystem이 정한다
    public static class InterpolationSystem
    {
        private const float DELAY_TICKS = 5.0f;   // 최신 스냅샷보다 5틱(100ms) 뒤를 그린다. 지터보다 커야 끊기지 않음
        private const float SNAP_TICKS = 10.0f;   // 이보다 멀어지면 보정 대신 순간이동
        private const float CORRECTION = 0.1f;    // 프레임당 오차를 당기는 비율

        public static void Receive(ref InterpolationClock clock, int tick)
        {
            if (!clock.HasTick)
            {
                clock.HasTick = true;
                clock.LatestTick = tick;
                clock.RenderTick = tick - DELAY_TICKS;
                return;
            }

            if (clock.LatestTick < tick)
            {
                clock.LatestTick = tick;
            }
        }

        public static void Advance(ref InterpolationClock clock, float frameTime)
        {
            if (!clock.HasTick) return;

            clock.RenderTick += frameTime / SimulationClock.DeltaTime;

            float target = clock.LatestTick - DELAY_TICKS;
            float error = target - clock.RenderTick;
            if (Mathf.Abs(error) > SNAP_TICKS)
            {
                clock.RenderTick = target;
            }
            else
            {
                clock.RenderTick += error * CORRECTION;
            }
        }
    }
}