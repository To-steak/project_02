using System.Collections.Generic;
using Project02.Player;
using Project02.Weapon;
using UnityEngine;

namespace Project02.Client
{
    // 원격 플레이어 표시: 자세(위치·회전·pitch), 애니메이션, 발사 연출
    public static class RemotePlayerViewSystem
    {
        public static void Present(ClientWorld world, float frameTime)
        {
            float renderTick = world.Interp.RenderTick;
            foreach (PlayerRemoteAuthoring player in world.Players.Values)
            {
                PresentPose(player, renderTick, frameTime);
                PresentFires(player, renderTick);
            }
        }

        private static void PresentPose(PlayerRemoteAuthoring player, float renderTick, float frameTime)
        {
            if (!SnapshotUtil.TrySample(player.Snapshots, renderTick, out PlayerSnapshot from, out PlayerSnapshot to, out float t)) return;

            Vector3 position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
            float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
            float pitch = Mathf.Lerp(from.Pitch, to.Pitch, t);
            player.transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));

            PlayerBodyView visual = player.Visual;
            visual.ApplyPitch(pitch);
            visual.AimAtPitch(pitch);

            // 이산값(접지, 달리기)은 가까운 쪽 스냅샷
            PlayerSnapshot nearest = t < 0.5f ? from : to;
            float verticalSpeed = Mathf.Lerp(from.Movement.VerticalSpeed, to.Movement.VerticalSpeed, t);
            Vector2 move = Vector2.Lerp(from.Move, to.Move, t);

            PlayerAnimatorView animation = player.Animation;
            animation.SetAirborne(nearest.Movement.IsGrounded, verticalSpeed);
            animation.SetMoveBlendTree(move, nearest.IsRunning, frameTime);
        }

        // 그리는 시점이 발사 틱에 도달했을 때 재생 (몸과 총알의 시점을 맞춤)
        private static void PresentFires(PlayerRemoteAuthoring player, float renderTick)
        {
            List<RemoteFire> fires = player.Network.Fires;
            WeaponView view = player.Weapon.View;

            for (int i = 0; i < fires.Count; i++)
            {
                RemoteFire fire = fires[i];
                if (fire.Tick > renderTick) continue;

                if (view != null)
                {
                    view.SpawnBullet(fire.AimPoint, fire.Spread);
                }

                fires.RemoveAt(i);
                i--;
            }
        }
    }
}