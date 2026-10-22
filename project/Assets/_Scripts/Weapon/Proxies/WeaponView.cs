using Unity.Cinemachine;
using UnityEngine;
using Project02.Combat;
using Project02.Player;
using Project02.Service;

namespace Project02.Weapon
{
    public class WeaponView : MonoBehaviour
    {
        public Vector3 Muzzle => _muzzle.position;
        public Transform RightGrip => _rightGrip;
        public Transform LeftGrip => _leftGrip;

        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _rightGrip;
        [SerializeField] private Transform _leftGrip;
        [SerializeField] private CinemachineImpulseSource _impulse;

        private SOWeapon _data;

        public void Initialize(SOWeapon data)
        {
            _data = data;
        }

        public void Shake(FireStance stance)
        {
            Shake(WeaponFeedbackUtil.StanceMultiplier(_data, stance), _data.ShakeRandom);
        }

        public void SpawnBullet(Vector3 aimPoint, Vector2 spread)
        {
            Vector3 from = Muzzle;
            Vector3 direction = WeaponFireUtil.GetDirection(from, aimPoint, spread);
            float distance = Mathf.Min(Vector3.Distance(from, aimPoint), _data.Range);
            ProjectileVisualService.Instance.Fire(from, from + direction * distance, _data.Bullet.Velocity);
        }

        // 예측 피격 연출 (ImpactViewSystem). 프리팹의 정본은 SOBullet.ImpactPrefab, 비어 있으면 아무것도 하지 않는다.
        // 벽·적 모두 같은 프리팹 (impact.Kind는 나중에 종류별 연출을 나눌 때 쓴다)
        public void SpawnImpact(in ImpactInfo impact)
        {
            GameObject prefab = _data.Bullet.ImpactPrefab;
            if (prefab == null) return;

            ProjectileVisualService.Instance.SpawnImpact(prefab, impact.Point, impact.Normal);
        }

        private void Shake(float multiplier, float random)
        {
            Vector3 velocity = _impulse.DefaultVelocity + (Vector3)(Random.insideUnitCircle * random);
            _impulse.GenerateImpulseWithVelocity(velocity * multiplier);
        }
    }
}