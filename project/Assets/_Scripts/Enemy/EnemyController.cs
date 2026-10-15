using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Project02.Server;

namespace Project02.Enemy
{
    public class EnemyController : NetworkBehaviour
    {
        internal EnemyStateId CurrentStateId { get; private set; }
        internal float StateTime;
        internal ulong? TargetClientId;
        internal Vector2 MoveInput;
        internal float MoveSpeed;
        internal SOEnemy Settings => _settings;
        internal Vector3 Position => _movement.Position;

        [SerializeField] private SOEnemy _settings;
        [SerializeField] private Health _health;

        private readonly List<int> _path = new();
        private readonly List<ulong> _contributors = new();
        private readonly NetworkVariable<EnemyStateId> _netStateId = new();

        private MovementState _movement;
        private Quaternion _rotation;

        private PathGrid _grid;
        private float _timer;
        private int _waypoint;

        private const float WANDER_INTERVAL = 1.0f;  // 도착 후 다음 배회까지 대기 시간
        private const float ARRIVE_THRESHOLD = 0.2f; // 도착 판정 거리 (진동 방지)

        private static readonly IEnemyState[] _states =
        {
        new GeneralState(),
        new NoticeState(),
        new BattleState(),
        new AttackState(),
        new GroggyState(),
        new DefendState(),
        new DieState()
        };

        public void InjectGrid(PathGrid grid)
        {
            Debug.Assert(grid.Profile == _settings.Profile, $"{name}: 그리드 프로필과 적 프로필이 다름", this);
            _grid = grid;
        }

        protected override void OnNetworkPostSpawn()
        {
            // TODO: Object Pool에서 나오면 초기화
            _path.Clear();
            _waypoint = 0;
            _movement = new MovementState { Position = transform.position };
            _rotation = transform.rotation;
            _timer = 0.0f;
            _contributors.Clear();

            if (IsServer)
            {
                TargetClientId = null;
                CurrentStateId = EnemyStateId.General;
                StateTime = 0.0f;
                _netStateId.Value = EnemyStateId.General;
                _states[(int)EnemyStateId.General].Enter(this);

                _health.Damaged += OnDamaged;
                _health.Died += OnDied;

                ServerSimulation.Instance.Register(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            // TODO: Object Pool에 반납할 때
            if (IsServer)
            {
                _health.Damaged -= OnDamaged;
                _health.Died -= OnDied;


                ServerSimulation.Instance.Unregister(this);
            }
        }

        public void Tick(float deltaTime)
        {
            if (_health.IsDead) return;

            StateTime += deltaTime;

            MoveInput = Vector2.zero;
            MoveSpeed = 0.0f;
            _states[(int)CurrentStateId].Tick(this, deltaTime);   // 상태 머신이 입력을 정함

            EnemyMovement.Step(_settings, ref _movement, ref _rotation, MoveInput, MoveSpeed, deltaTime);
            transform.SetPositionAndRotation(_movement.Position, _rotation);   // 출력만
        }

        internal Vector2 WanderInput(float time)
        {
            if (_waypoint >= _path.Count)
            {
                _timer += time;
                if (_timer >= WANDER_INTERVAL)
                {
                    _timer = 0.0f;
                    Wander();
                }
            }

            if (_waypoint < _path.Count)
            {
                Vector3 direction = _grid.ConvertWorldCoord(_path[_waypoint]) - _movement.Position;
                direction.y = 0.0f;
                if (direction.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD)
                {
                    _waypoint++;
                }
                else
                {
                    return new Vector2(direction.x, direction.z).normalized;
                }
            }

            return Vector2.zero;
        }

        private void Wander()
        {
            _path.Clear();
            _waypoint = 0;

            if (!_grid.TryConvertCellCoord(_movement.Position, out int startX, out int startZ))
            {
                return;
            }

            if (!_grid.TryGetRandomCell(out int goalX, out int goalZ))
            {
                return;
            }

            if (PathFinder.TryFindPath(_grid, startX, startZ, goalX, goalZ, _path))
            {
                PathFinder.Smooth(_grid, _path);
                _waypoint = _path.Count > 1 ? 1 : 0;
            }
        }

        private void OnDamaged(Health health, in DamageInfo info, int previous)
        {
            if (info.Source == DamageSource.Player && !_contributors.Contains(info.AttackerId))
            {
                _contributors.Add(info.AttackerId);
            }

            if (health.IsDead) return;              // 1. Die는 Died 이벤트에서
            if (CurrentStateId == EnemyStateId.Defend) return; // 2. 피해만 감소, 경직 없음
        }

        private void OnDied(Health health)
        {
            ServerSimulation.Instance.EnqueueDeath(this);
        }

        internal void Die()
        {
            int count = _contributors.Count;
            if (count > 0)
            {
                int exp = _settings.RewardExp / count;
                int gold = _settings.RewardGold / count;

                SessionService.Instance.AddReward(_contributors, exp, gold);
            }

            NetworkObject.Despawn();
        }

        internal void ChangeState(EnemyStateId nextStateId)
        {
            _states[(int)CurrentStateId].Exit(this);
            CurrentStateId = nextStateId;
            StateTime = 0.0f;
            _netStateId.Value = nextStateId;
            _states[(int)nextStateId].Enter(this);
        }

        internal bool TryGetTarget(out Transform target, out Health health)
        {
            target = null;
            health = null;

            if (!TargetClientId.HasValue || !NetworkManager.ConnectedClients.TryGetValue(TargetClientId.Value, out var client) || client.PlayerObject == null)
            {
                return false;
            }

            target = client.PlayerObject.transform;
            _ = client.PlayerObject.TryGetComponent(out health);

            return health != null && !health.IsDead;
        }

        internal bool TryFindClosestPlayer(float range, out ulong clientId)
        {
            clientId = default;
            float best = range * range;
            bool found = false;

            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null || !player.TryGetComponent(out Health health) || health.IsDead)
                {
                    continue;
                }

                Vector3 offset = player.transform.position - _movement.Position;
                offset.y = 0.0f;
                float sqr = offset.sqrMagnitude;
                if (sqr <= best)
                {
                    best = sqr;
                    clientId = client.ClientId;
                    found = true;
                }
            }

            return found;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.0f, $"{CurrentStateId}  target:{TargetClientId?.ToString() ?? "-"}");

            if (_settings != null)
            {
                DrawCapsule();
                DrawRange(_settings.BattleRange, new Color(0.2f, 0.4f, 1.0f));
                DrawRange(_settings.PerceptionRange, new Color(1.0f, 0.85f, 0.2f));
                DrawRange(_settings.AttackRange, new Color(1.0f, 0.25f, 0.25f));
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
                Gizmos.color = _movement.IsGrounded ? Color.yellow : Color.gray;
                Gizmos.DrawLine(position, position + Vector3.down * (radius * 0.5f));
            }
        }

        private static void DrawCircle(Vector3 center, float radius, int segments = 48)
        {
            Vector3 previous = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        private void DrawRange(float radius, Color color)
        {
            Vector3 center = transform.position + Vector3.up * 0.02f;

            UnityEditor.Handles.color = new Color(color.r, color.g, color.b, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(center, Vector3.up, radius);

            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawWireDisc(center, Vector3.up, radius);
        }
#endif
    }
}