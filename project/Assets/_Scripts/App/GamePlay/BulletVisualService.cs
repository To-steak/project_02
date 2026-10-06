using UnityEngine;
using UnityEngine.Pool;

public class BulletVisualService : MonoBehaviour, IBulletVisualService
{
    [SerializeField] private Bullet _prefab;
    [SerializeField] private int _defaultCapacity = 64;
    [SerializeField] private int _maxSize = 256;

    private ObjectPool<Bullet> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Bullet>(
            createFunc: () => Instantiate(_prefab, transform).Inject(_pool),
            actionOnGet: bullet => bullet.gameObject.SetActive(true),
            actionOnRelease: bullet => bullet.gameObject.SetActive(false),
            actionOnDestroy: bullet => Destroy(bullet.gameObject),
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );

        GameServices.Register(this);
    }

    private void OnDestroy()
    {
        GameServices.Unregister(this);
        _pool.Dispose();
    }

    public void Fire(Vector3 from, Vector3 to, float speed)
    {
        _pool.Get().Launch(from, to, speed);
    }
}
