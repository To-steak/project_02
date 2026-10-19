using System.Collections.Generic;
using Project02.Character;
using Project02.Weapon;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerView : MonoBehaviour
    {
        private readonly struct FireEvent
        {
            public readonly ShotInfo Shot;
            public readonly Vector3 Point;

            public FireEvent(in ShotInfo shot, Vector3 point)
            {
                Shot = shot;
                Point = point;
            }
        }

        [SerializeField] private PlayerBodyView _visual;
        [SerializeField] private PlayerAnimatorView _animation;
        [SerializeField] private PlayerWeaponView _weapon;

        private readonly List<FireEvent> _fired = new();

        public void OnTick(in MovementState movement)
        {
            _visual.Record(movement.Position);
        }

        public void OnFired(in ShotInfo shot, Vector3 point)
        {
            _fired.Add(new FireEvent(shot, point));
        }

        // LateUpdate
        public void Present(float yaw, float pitch, Vector3 point, Vector2 move, in MovementState movement, bool isRunning, float alpha, float deltaTime)
        {
            // 1. 위치 보간
            _visual.Interpolate(alpha);

            // 2. 즉시 회전
            _visual.ApplyPitch(pitch);
            _visual.ApplyYaw(yaw);

            // 3. 상체 각도, 조준, 바라보기
            _visual.AimAt(point);
            _visual.Face(point, _weapon.Muzzle, deltaTime);

            // 4. 애니메이션
            _animation.SetAirborne(movement.IsGrounded, movement.VerticalSpeed);
            _animation.SetMoveBlendTree(move, isRunning, deltaTime);

            // 5. _fired 연출 (프리팹 없는 무기는 뷰가 없다)
            WeaponView view = _weapon.View;
            if (view != null)
            {
                foreach (var fired in _fired)
                {
                    view.SpawnBullet(fired.Point, fired.Shot.Spread);
                    view.Shake(fired.Shot.Stance);
                }
            }

            _fired.Clear();
        }
    }
}