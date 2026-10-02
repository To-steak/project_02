using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    private IObjectPool<Bullet> _pool;
    private Vector3 _target;
    private float _speed;
    private const float MIN_DISTANCE = 0.001f; // 1mm
    private const float MIN_DISTANCE_SQR = MIN_DISTANCE * MIN_DISTANCE;

    public Bullet Initialize(IObjectPool<Bullet> pool)
    {
        _pool = pool;

        return this;
    }

    public void Launch(Vector3 from, Vector3 to, float speed)
    {
        Vector3 direction = to - from;
        transform.SetPositionAndRotation(from, direction.sqrMagnitude > MIN_DISTANCE_SQR ? Quaternion.LookRotation(direction) : Quaternion.identity);
        _target = to;
        _speed = speed;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
        if (transform.position == _target)
        {
            _pool.Release(this);
        }
    }
}
