using Project02.Character;
using Project02.Enemy;
using UnityEngine;

namespace Project02.Client
{
    public static class InterpolationSystem
    {
        private const float DELAY_TICKS = 5.0f;   // 최신 스냅샷보다 5틱(100ms) 뒤를 그린다. 지터보다 커야 끊기지 않음
        private const float SNAP_TICKS = 10.0f;   // 이보다 멀어지면 보정 대신 순간이동
        private const float CORRECTION = 0.1f;    // 프레임당 오차를 당기는 비율
        private const int SEARCH_RANGE = EnemyRemote.BUFFER_SIZE / 2;

        // 수신 시: 시계에 새 틱을 알린다
        public static void Receive(ref InterpolationClock clock, int tick)
        {
            if (!clock.HasTick)
            {
                clock.HasTick = true;
                clock.LatestTick = tick;
                clock.RenderTick = tick - DELAY_TICKS;
                return;
            }

            if (tick > clock.LatestTick)
            {
                clock.LatestTick = tick;   // 순서가 뒤바뀌어 온 옛 틱은 무시
            }
        }

        // 매 프레임: RenderTick을 실시간으로 흘리고, 목표 지점으로 살짝 당긴다 (예측 시계의 dilation과 같은 발상)
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

        // 적 하나: RenderTick을 사이에 둔 두 스냅샷을 찾아 보간
        public static void Present(EnemyRemote enemy, float renderTick)
        {
            int baseTick = Mathf.FloorToInt(renderTick);

            // from: renderTick 이하에서 가장 가까운 스냅샷 (유실 대비 뒤로 탐색)
            if (!TryFindBackward(enemy, baseTick, out int fromTick, out EnemySnapshot from))
            {
                return;   // 아직 받은 게 없음. 스폰 위치 그대로
            }

            // to: renderTick 초과에서 가장 가까운 스냅샷 (유실 대비 앞으로 탐색)
            Vector3 position;
            float yaw;
            if (TryFindForward(enemy, baseTick + 1, out int toTick, out EnemySnapshot to))
            {
                float t = (renderTick - fromTick) / (toTick - fromTick);
                position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
                yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
            }
            else
            {
                // 다음 스냅샷이 아직 없음: 마지막 것을 유지 (외삽하지 않음)
                position = from.Movement.Position;
                yaw = from.Yaw;
            }

            enemy.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));
            enemy.ViewStateId = from.StateId;   // 이산값은 보간 불가 → 과거 쪽 값을 그대로
            enemy.ViewMovement = from.Movement; // 애니메이션용 (VerticalSpeed, IsGrounded)
        }

        private static bool TryFindBackward(EnemyRemote enemy, int tick, out int found, out EnemySnapshot snapshot)
        {
            for (int i = 0; i < SEARCH_RANGE; i++)
            {
                int candidate = tick - i;
                int index = candidate & EnemyRemote.BUFFER_MASK;
                if (enemy.Ticks[index] == candidate)
                {
                    found = candidate;
                    snapshot = enemy.Snapshots[index];
                    return true;
                }
            }

            found = -1;
            snapshot = default;
            return false;
        }

        private static bool TryFindForward(EnemyRemote enemy, int tick, out int found, out EnemySnapshot snapshot)
        {
            for (int i = 0; i < SEARCH_RANGE; i++)
            {
                int candidate = tick + i;
                int index = candidate & EnemyRemote.BUFFER_MASK;
                if (enemy.Ticks[index] == candidate)
                {
                    found = candidate;
                    snapshot = enemy.Snapshots[index];
                    return true;
                }
            }

            found = -1;
            snapshot = default;
            return false;
        }
    }
}