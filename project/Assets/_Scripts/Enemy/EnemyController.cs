using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif

public class EnemyController : MonoBehaviour
{
    public float Timer;
    [SerializeField] private EnemySettings _settings;

    private Vector3 _destination;
    private bool _hasDestination;
    private bool _isGrounded;
    private float _fallSpeed;

    private const int MAX_ATTEMPTS = 8;          // 목적지 후보를 최대 몇 번 뽑아볼지
    private const float MIN_RAY_LENGTH = 1.0f;   // 배회 반경 최소 (이보다 가까우면 후보 기각)
    private const float MAX_RAY_LENGTH = 4.0f;   // 배회 반경 최대
    private const float SKIN = 0.2f;             // 벽에 박히지 않도록 떨어뜨릴 거리
    private const float EYE_HEIGHT = 0.5f;       // 수평 Ray 발사 높이 (턱, 경사 오판 방지)
    private const float MAX_STEP_UP = 3.0f;      // 후보 지점에서 오를 수 있는 최대 높이
    private const float MAX_STEP_DOWN = 3.0f;    // 후보 지점에서 내려갈 수 있는 최대 높이
    private const float WANDER_INTERVAL = 3.0f;  // 도착 후 다음 배회까지 대기 시간
    private const float ARRIVE_THRESHOLD = 0.5f; // 도착 판정 거리 (진동 방지)

    private void OnEnable()
    {
        // TODO: Object Pool에서 나오면 초기화
        _destination = transform.position;
        _hasDestination = false;
        _isGrounded = false;
        _fallSpeed = 0.0f;
        Timer = 0.0f;

#if UNITY_EDITOR
        _debugRays.Clear();
        _debugPoints.Clear();
#endif
    }


    private void FixedUpdate()
    {
        float time = Time.fixedDeltaTime;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        if (!_hasDestination)
        {
            Timer += time;
        }

        if (Timer >= WANDER_INTERVAL)
        {
            Timer = 0.0f;
            _hasDestination = TryGetDestination(out _destination);
        }

        Vector2 input = Vector2.zero;

        if (_hasDestination)
        {
            Vector3 direction = _destination - position;
            direction.y = 0.0f;

            if (direction.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD)
            {
                _hasDestination = false;
            }
            else
            {
                input = new Vector2(direction.x, direction.z).normalized;
                Vector3 move = CharacterPhysics.Move(position, input, _settings.WalkSpeed, time);
                position = CharacterPhysics.Walk(position, move, _settings.Radius, _settings.Height, _settings.SlopeLimit, _settings.StepHeight, _settings.GroundLayer, _isGrounded, out _);
            }
        }

        _fallSpeed = CharacterPhysics.ApplyGravity(_fallSpeed, _settings.Gravity, _settings.MaxFallSpeed, time);
        Vector3 fall = CharacterPhysics.Fall(position, _fallSpeed, time);
        position = CharacterPhysics.Collide(position, fall, _settings.Radius, _settings.Height, _settings.GroundLayer);

        _isGrounded = CharacterPhysics.IsGrounded(fall, position, _fallSpeed);
        if (_isGrounded) _fallSpeed = _settings.GroundStickSpeed;

        rotation = input != Vector2.zero ? CharacterPhysics.Rotate(rotation, input, _settings.RotationSpeed, time) : rotation;

        transform.SetPositionAndRotation(position, rotation);
    }

    private void Update()
    {
        // 애니메이션 로직
    }

    private void OnDisable()
    {
        // Object Pool에 반환될 때
    }

    private void OnDestroy()
    {
        // 파괴될 때
    }

    private bool TryGetDestination(out Vector3 destination)
    {
#if UNITY_EDITOR
        _debugRays.Clear();
        _debugPoints.Clear();
#endif

        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            // 해당 값은 테스트 전용 임시 값으로 나중에 서버에서 받아야 한다. 아니면 각 클라마다 결과가 다름.
            float random = Random.Range(0f, Mathf.PI * 2f);
            Vector3 direction = new Vector3(Mathf.Cos(random), 0f, Mathf.Sin(random));
            float length = Random.Range(MIN_RAY_LENGTH, MAX_RAY_LENGTH);

            Vector3 source = transform.position + Vector3.up * EYE_HEIGHT;
            bool blocked = Physics.Raycast(source, direction, out RaycastHit obstacle, length, _settings.ObstacleLayer);

#if UNITY_EDITOR
            // 수평 레이: 원래 뽑은 길이를 흰 실선으로 먼저 그려 둔다
            AddRay(source, direction, length, ColorWhiteFaint);
#endif

            if (blocked)
            {
#if UNITY_EDITOR
                // 막힌 지점까지를 빨강으로 덮어 그리고, 충돌점에 X 표시
                AddRay(source, direction, obstacle.distance, Color.red);
                AddPoint(obstacle.point, Color.red, 0.12f);
#endif
                length = obstacle.distance - SKIN;
            }

            if (length < MIN_RAY_LENGTH)
            {
#if UNITY_EDITOR
                // 너무 짧아서 버려진 시도 = 회색
                AddRay(source, direction, MIN_RAY_LENGTH, Color.gray);
#endif
                continue;
            }

            Vector3 candidate = transform.position + direction * length;
            Vector3 groundOrigin = candidate + Vector3.up * MAX_STEP_UP;
            bool grounded = Physics.Raycast(groundOrigin, Vector3.down, out RaycastHit ground, MAX_STEP_UP + MAX_STEP_DOWN, _settings.GroundLayer);

#if UNITY_EDITOR
            AddRay(groundOrigin, Vector3.down, MAX_STEP_UP + MAX_STEP_DOWN, grounded ? Color.green : Color.magenta);
#endif

            if (!grounded)
            {
#if UNITY_EDITOR
                // 땅이 없어서 버려진 후보 = 마젠타
                AddPoint(candidate, Color.magenta, 0.15f);
#endif
                continue;
            }

            destination = ground.point;

#if UNITY_EDITOR
            // 채택된 수평 레이 = 노랑, 확정 좌표 = 초록 구
            AddRay(source, direction, length, Color.yellow);
            AddPoint(ground.point, Color.green, 0.2f);
#endif
            return true;
        }

        destination = transform.position;
        return false;
    }

#if UNITY_EDITOR
    private static readonly Color ColorWhiteFaint = new Color(1f, 1f, 1f, 0.25f);

    private struct DebugRay
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public float Length;
        public Color Color;
    }

    private struct DebugPoint
    {
        public Vector3 Position;
        public Color Color;
        public float Radius;
    }

    private readonly List<DebugRay> _debugRays = new List<DebugRay>();
    private readonly List<DebugPoint> _debugPoints = new List<DebugPoint>();

    private void AddRay(Vector3 origin, Vector3 direction, float length, Color color)
    {
        _debugRays.Add(new DebugRay
        {
            Origin = origin,
            Direction = direction,
            Length = length,
            Color = color
        });
    }

    private void AddPoint(Vector3 position, Color color, float radius)
    {
        _debugPoints.Add(new DebugPoint
        {
            Position = position,
            Color = color,
            Radius = radius
        });
    }

    private void OnDrawGizmosSelected()
    {
        // 마지막 탐색에서 쏜 레이들
        for (int i = 0; i < _debugRays.Count; i++)
        {
            DebugRay ray = _debugRays[i];
            Gizmos.color = ray.Color;
            Gizmos.DrawLine(ray.Origin, ray.Origin + ray.Direction * ray.Length);
        }

        for (int i = 0; i < _debugPoints.Count; i++)
        {
            DebugPoint point = _debugPoints[i];
            Gizmos.color = point.Color;
            Gizmos.DrawSphere(point.Position, point.Radius);
        }

        // 레이 길이 범위를 바닥에 원으로 표시
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.35f);
        DrawCircle(transform.position, MIN_RAY_LENGTH);
        DrawCircle(transform.position, MAX_RAY_LENGTH);

        // 현재 목적지와 도착 판정 반경
        if (_hasDestination)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, _destination + Vector3.up * 0.1f);
            Gizmos.DrawWireSphere(_destination, ARRIVE_THRESHOLD);
        }

        // 눈높이(수평 레이 시작점) 표시
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * EYE_HEIGHT, 0.06f);
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