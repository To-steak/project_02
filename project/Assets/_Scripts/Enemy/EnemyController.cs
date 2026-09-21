using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public float Timer;
    [SerializeField] private EnemySettings _settings;
    [SerializeField] private PathGrid _grid;

    private bool _isGrounded;
    private float _verticalSpeed;

    private readonly List<int> _path = new();
    private int _waypoint;

    private const float WANDER_INTERVAL = 1.0f;  // 도착 후 다음 배회까지 대기 시간
    private const float ARRIVE_THRESHOLD = 0.2f; // 도착 판정 거리 (진동 방지)

    private void OnEnable()
    {
        // TODO: Object Pool에서 나오면 초기화
        _path.Clear();
        _waypoint = 0;
        _isGrounded = false;
        _verticalSpeed = 0.0f;
        Timer = 0.0f;
    }


    private void FixedUpdate()
    {
        float time = Time.fixedDeltaTime;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        LayerMask layer = _settings.Profile.GroundLayer | _settings.Profile.ObstacleLayer;

        if (_waypoint >= _path.Count)
        {
            Timer += time;
            if (Timer >= WANDER_INTERVAL)
            {
                Timer = 0.0f;
                Wander();
            }
        }

        Vector2 input = Vector2.zero;
        if (_waypoint < _path.Count)
        {
            Vector3 direction = _grid.ConvertWorldCoord(_path[_waypoint]) - position;
            direction.y = 0.0f;
            if (direction.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD)
            {
                _waypoint++;
            }
            else
            {
                input = new Vector2(direction.x, direction.z).normalized;
                Vector3 move = new Vector3(input.x, 0.0f, input.y) * _settings.WalkSpeed * time;
                position = CharacterPhysics.Walk(position, position + move, _settings.Profile.Radius, _settings.Profile.Height, _settings.Profile.SlopeLimit, _settings.Profile.StepHeight, layer, _isGrounded, out _);
            }
        }

        _verticalSpeed = CharacterPhysics.ApplyGravity(_verticalSpeed, _settings.Profile.Gravity, _settings.Profile.MaxFallSpeed, time);
        Vector3 verticalPosition = position + Vector3.up * _verticalSpeed * time;
        
        position = CharacterPhysics.Collide(position, verticalPosition, _settings.Profile.Radius, _settings.Profile.Height, layer);

        _isGrounded = CharacterPhysics.IsGrounded(verticalPosition, position, _verticalSpeed);
        if (_isGrounded) _verticalSpeed = _settings.Profile.GroundStickSpeed;

        rotation = input != Vector2.zero ? CharacterPhysics.Rotate(rotation, input, _settings.RotationSpeed, time) : rotation;

        transform.SetPositionAndRotation(position, rotation);
    }

    // private void Update()
    // {
    //     // 애니메이션 로직
    // }

    // private void OnDisable()
    // {
    //     // Object Pool에 반환될 때
    // }

    // private void OnDestroy()
    // {
    //     // 파괴될 때
    // }

    private void Wander()
    {
        _path.Clear();
        _waypoint = 0;

        if (!_grid.TryConvertCellCoord(transform.position, out int startX, out int startZ))
        {
            return;
        }

        if (!_grid.TryGetRandomCell(out int goalX, out int goalZ))
        {
            return;
        }

        PathFinder.TryFindPath(_grid, startX, startZ, goalX, goalZ, _path);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_settings != null)
        {
            DrawCapsule();
        }

        if (_grid == null || _waypoint >= _path.Count)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        for (int i = _waypoint; i < _path.Count; i++)
        {
            Vector3 point = _grid.ConvertWorldCoord(_path[i]) + Vector3.up * 0.2f;
            Gizmos.DrawSphere(point, 0.08f);

            Vector3 previous = i == _waypoint ? transform.position : _grid.ConvertWorldCoord(_path[i - 1]) + Vector3.up * 0.2f;
            Gizmos.DrawLine(previous, point);
        }

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        DrawCircle(transform.position, ARRIVE_THRESHOLD);
    }

    private void DrawCapsule()
    {
        float radius = _settings.Profile.Radius;
        Vector3 position = transform.position;
        LayerMask layer = _settings.Profile.GroundLayer | _settings.Profile.ObstacleLayer;

        CharacterPhysics.GetCapsule(position, radius, _settings.Profile.Height, out Vector3 bottom, out Vector3 top);

        bool overlapped = Physics.CheckCapsule(bottom, top, radius, layer, QueryTriggerInteraction.Ignore);
        Gizmos.color = overlapped ? Color.red : Color.green;

        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawWireSphere(top, radius);

        Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
        Gizmos.DrawLine(bottom + Vector3.left * radius, top + Vector3.left * radius);
        Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
        Gizmos.DrawLine(bottom + Vector3.back * radius, top + Vector3.back * radius);

        // 올라설 수 있는 최대 턱 높이
        Vector3 step = position + Vector3.up * _settings.Profile.StepHeight;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(step + Vector3.right * radius, step + Vector3.forward * radius);
        Gizmos.DrawLine(step + Vector3.forward * radius, step + Vector3.left * radius);
        Gizmos.DrawLine(step + Vector3.left * radius, step + Vector3.back * radius);
        Gizmos.DrawLine(step + Vector3.back * radius, step + Vector3.right * radius);

        if (Application.isPlaying)
        {
            Gizmos.color = _isGrounded ? Color.yellow : Color.gray;
            Gizmos.DrawLine(position, position + Vector3.down * (radius * 0.5f));
        }
    }

    private static void DrawCircle(Vector3 center, float radius, int segments = 48)
    {
        Vector3 previous = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
#endif
}