using Unity.Cinemachine;
using UnityEngine;
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
            Shake(WeaponFeedback.StanceMultiplier(_data, stance), _data.ShakeRandom);
        }

        public void SpawnBullet(Vector3 aimPoint, Vector2 spread)
        {
            Vector3 from = Muzzle;
            Vector3 direction = PlayerWeaponView.GetDirection(from, aimPoint, spread);
            float distance = Mathf.Min(Vector3.Distance(from, aimPoint), _data.Range);
            ProjectileVisualService.Instance.Fire(from, from + direction * distance, _data.Bullet.Velocity);
        }

        private void Shake(float multiplier, float random)
        {
            Vector3 velocity = _impulse.DefaultVelocity + (Vector3)(Random.insideUnitCircle * random);
            _impulse.GenerateImpulseWithVelocity(velocity * multiplier);
        }
    }
}