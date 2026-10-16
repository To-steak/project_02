using Project02.Core;
using Project02.Enemy;
using Project02.Player;
using Project02.Weapon;
using UnityEngine;

namespace Project02.Client
{
    public static class InterpolationSystem
    {
        private const float DELAY_TICKS = 5.0f;
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

        public static void Present(EnemyRemote enemy, float renderTick)
        {
            if (!TrySample(enemy.Snapshots, renderTick, out EnemySnapshot from, out EnemySnapshot to, out float t)) return;

            Vector3 position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
            float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
            enemy.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));
        }

        public static void Present(PlayerRemote player, float renderTick, float frameTime)
        {
            PlayerController controller = player.Controller;

            // 1. 위치·회전·상체 각도·애니메이션
            if (TrySample(player.Snapshots, renderTick, out PlayerSnapshot from, out PlayerSnapshot to, out float t))
            {
                Vector3 position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
                float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
                float pitch = Mathf.Lerp(from.Pitch, to.Pitch, t);
                controller.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));

                PlayerVisual visual = controller.PlayerVisual;
                visual.ApplyPitch(pitch);
                visual.AimAtPitch(pitch);

                // 이산값(접지, 달리기)은 가까운 쪽 스냅샷
                PlayerSnapshot nearest = t < 0.5f ? from : to;
                float verticalSpeed = Mathf.Lerp(from.Movement.VerticalSpeed, to.Movement.VerticalSpeed, t);
                Vector2 move = Vector2.Lerp(from.Move, to.Move, t);

                PlayerAnimation animation = controller.PlayerAnimation;
                animation.SetAirborne(nearest.Movement.IsGrounded, verticalSpeed);
                animation.SetMoveBlendTree(move, nearest.IsRunning, frameTime);
            }

            // 2. 발사 연출: 그리는 시점이 발사 틱에 도달했을 때 (몸과 총알의 시점을 맞춤)
            var fires = player.Fires;
            for (int i = 0; i < fires.Count; i++)
            {
                RemoteFire fire = fires[i];
                if (fire.Tick > renderTick) continue;

                WeaponView view = controller.PlayerWeapon.View;
                if (view != null)
                {
                    view.SpawnBullet(fire.AimPoint, fire.Spread);
                }

                fires.RemoveAt(i);
                i--;
            }
        }

        // RenderTick 앞뒤 스냅샷 찾기. 유실된 틱은 건너뛰고 가장 가까운 것을 쓴다
        private static bool TrySample<T>(TickBuffer<T> buffer, float renderTick, out T from, out T to, out float t) where T : struct
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