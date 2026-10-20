using UnityEngine;
using Project02.Core;

namespace Project02.World
{
    public static class ClockSystem
    {
        public const float TARGET_BUFFER = 3.0f;
        
        private const float MAX_DILATION = 0.05f;
        private const float DILATION_GAIN = 0.025f;
        private const int MAX_TICKS_PER_FRAME = 5;

        public static ClientClock Create() => new ClientClock
        {
            Tick = 0,
            Accumulator = 0.0f,
            Interval = SimulationClock.DeltaTime,
            SmoothedBuffer = TARGET_BUFFER,
            RttTicks = -1.0f
        };

        public static int Advance(ref ClientClock clock, float frameTime)
        {
            // 버퍼가 모자라면 짧게(빨리), 남으면 길게(느리게)
            float error = TARGET_BUFFER - clock.SmoothedBuffer;
            float scale = 1.0f - Mathf.Clamp(error * DILATION_GAIN, -MAX_DILATION, MAX_DILATION);
            clock.Interval = SimulationClock.DeltaTime * scale;

            clock.Accumulator += frameTime;
            int ticks = 0;
            while (clock.Accumulator >= clock.Interval && ticks < MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator -= clock.Interval;
                ticks++;
            }

            if (ticks == MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator = 0.0f;
            }

            return ticks;
        }
    }
}