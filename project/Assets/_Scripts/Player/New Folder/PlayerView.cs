using System.Collections.Generic;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerView : MonoBehaviour
    {
        private readonly struct FireEvent
        {
            public readonly Shot Shot;
            public readonly Vector3 Point;

            public FireEvent(in Shot shot, Vector3 point)
            {
                Shot = shot;
                Point = point;
            }
        }

        [SerializeField] private PlayerVisual _visual;
        [SerializeField] private PlayerAnimation _animation;
        [SerializeField] private PlayerWeapon _weapon;

        private readonly List<FireEvent> _fired = new();

        public void OnTick(in MovementState movement)
        {
            _visual.Record(movement.Position);
        }

        public void OnFired(in Shot shot, Vector3 point)
        {
            _fired.Add(new FireEvent(shot, point));
        }

        // LateUpdate
        public void Present(float yaw, float pitch, Vector3 point, Vector2 move, in MovementState movement, bool isRunning, float deltaTime)
        {
            // 1. 위치 보간
            _visual.Interpolate();

            // 2. 즉시 회전
            _visual.ApplyPitch(pitch);
            _visual.ApplyYaw(yaw);
            
            // 3. 상체 각도, 조준, 바라보기
            _visual.AimAt(point);
            _visual.Face(point, _weapon.Muzzle, deltaTime);

            // 4. 애니메이션
            _animation.SetAirborne(movement.IsGrounded, movement.VerticalSpeed);
            _animation.SetMoveBlendTree(move, isRunning, deltaTime);

            // 5. _fired 연출
            WeaponView view = _weapon.View;
            foreach (var fired in _fired)
            {
                view.SpawnBullet(fired.Point, fired.Shot.Spread);
                view.Shake(fired.Shot.Stance);
            }

            _fired.Clear();
        }
    }
}