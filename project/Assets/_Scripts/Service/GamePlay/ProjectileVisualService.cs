using Project02.Weapon;
using UnityEngine;
using UnityEngine.Pool;

namespace Project02.Service
{
    public class ProjectileVisualService : MonoBehaviour
    {
        public static ProjectileVisualService Instance { get; private set; }

        [SerializeField] private BulletView _prefab;
        [SerializeField] private int _defaultCapacity = 64;
        [SerializeField] private int _maxSize = 256;

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
    }
}