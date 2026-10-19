using Project02.Enemy;
using UnityEngine;

namespace Project02.Client
{
    // 원격 적 표시: RenderTick 시점의 위치·회전
    public static class EnemyViewSystem
    {
        public static void Present(ClientWorld world)
        {
            float renderTick = world.Interp.RenderTick;
            foreach (EnemyRemoteAuthoring enemy in world.Enemies.Values)
            {
                if (!SnapshotUtil.TrySample(enemy.Snapshots, renderTick, out EnemySnapshot from, out EnemySnapshot to, out float t)) continue;

                Vector3 position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
                float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
                enemy.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));
            }
        }
    }
}