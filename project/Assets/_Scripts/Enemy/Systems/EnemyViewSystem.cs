using UnityEngine;
using Project02.World;

namespace Project02.Enemy
{
    // 원격 적 표시: 그린 시점(world.ViewTick)의 위치·회전.
    // 되감기 상한을 넘는 지연이면 ViewTick이 받은 스냅샷보다 앞설 수 있다 → 마지막 두 스냅샷의 위치 차이로 외삽 (ExtrapolationUtil).
    // 원격 플레이어는 판정 대상이 아니므로 외삽하지 않는다 (RemotePlayerViewSystem은 RenderTick)
    public static class EnemyViewSystem
    {
        public static void Present(ClientWorld world)
        {
            if (!world.Interp.HasTick) return;   // 스냅샷 전 ViewTick = -1: 샘플할 시점이 없다

            float viewTick = world.ViewTick;
            bool extrapolating = world.ViewExtrapolation > 0.0f;

            foreach (EnemyRemoteAuthoring enemy in world.Enemies.Values)
            {
                Vector3 position;
                float yaw;

                if (extrapolating && ExtrapolationUtil.TryGetLastTwo(enemy.Snapshots, viewTick, out EnemySnapshot prev, out EnemySnapshot last, out float k))
                {
                    position = Vector3.LerpUnclamped(prev.Movement.Position, last.Movement.Position, k);
                    yaw = last.Yaw;   // 회전은 외삽하지 않는다
                }
                else if (SnapshotUtil.TrySample(enemy.Snapshots, viewTick, out EnemySnapshot from, out EnemySnapshot to, out float t))
                {
                    position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
                    yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
                }
                else
                {
                    continue;
                }

                enemy.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));
            }
        }
    }
}