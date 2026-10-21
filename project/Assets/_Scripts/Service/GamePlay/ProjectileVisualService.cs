using UnityEngine;
using UnityEngine.Pool;
using Project02.Weapon;

namespace Project02.Service
{
    public class ProjectileVisualService : MonoBehaviour
    {
        public static ProjectileVisualService Instance { get; private set; }

        [SerializeField] private BulletView _prefab;
        [SerializeField] private int _defaultCapacity = 64;
        [SerializeField] private int _maxSize = 256;
        [SerializeField] private float _impactLifetime = 2.0f;   // 피격 이펙트를 지우기까지 (초)

        private ObjectPool<BulletView> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<BulletView>(
                createFunc: () => Instantiate(_prefab, transform).Inject(_pool),
                actionOnGet: bullet => bullet.gameObject.SetActive(true),
                actionOnRelease: bullet => bullet.gameObject.SetActive(false),
                actionOnDestroy: bullet => Destroy(bullet.gameObject),
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize
            );

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            _pool.Dispose();
        }

        public void Fire(Vector3 from, Vector3 to, float speed)
        {
            _pool.Get().Launch(from, to, speed);
        }

        // 피격 이펙트. 프리팹은 부르는 쪽(WeaponView)이 SOBullet.ImpactPrefab에서 꺼내 넘긴다
        public void SpawnImpact(GameObject prefab, Vector3 point, Vector3 normal)
        {
            Quaternion rotation = normal.sqrMagnitude > 0.0f ? Quaternion.LookRotation(normal) : Quaternion.identity;
            GameObject impact = Instantiate(prefab, point, rotation, transform);
            Destroy(impact, _impactLifetime);
        }
    }
}