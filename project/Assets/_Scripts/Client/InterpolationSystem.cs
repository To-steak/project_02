using Project02.Core;
using Project02.Enemy;
using UnityEngine;

namespace Project02.Client
{
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
            if (!clock.HasTick)
            {
                return;
            }

            clock.RenderTick += frameTime / SimulationClock.DeltaTime;

            float target = clock.LatestTick - DELAY_TICKS;
            var error = target - clock.RenderTick;
            if (Mathf.Abs(error) > SNAP_TICKS)
            {
                clock.RenderTick = target;
            }
            else
            {
                clock.RenderTick += error * CORRECTION;
            }
        }

        public static void Present(EnemyRemote enemy, float renderTick)
        {
            int standard = Mathf.FloorToInt(renderTick);
            int range = enemy.Snapshots.Capacity / 2;

            int fromTick = -1;
            EnemySnapshot from = default;
            for (int i = 0; i < range; i++)
            {
                int candidate = standard - i;
                if (enemy.Snapshots.TryGetValue(candidate, out from))
                {
                    fromTick = candidate;
                    break;
                }
            }

            if (fromTick < 0)
            {
                return;
            }

            int toTick = fromTick;
            EnemySnapshot to = default;
            for (int j = 1; j < range; j++)
            {
                int candidate = standard + j;
                if (enemy.Snapshots.TryGetValue(candidate, out to))
                {
                    toTick = candidate;
                    break;
                }
            }

            float t = toTick == fromTick ? 0.0f : (renderTick - fromTick) / (toTick - fromTick);
            Vector3 position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
            float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
            enemy.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));
        }
    }
}