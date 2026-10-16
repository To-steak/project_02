using UnityEngine;
using Unity.Netcode;
using Project02.Server;
using Project02.Character;
using Project02.Combat;
using Project02.Navigation;

namespace Project02.Enemy
{
    // Authoring: 설정·그리드 주입, 등록, 기즈모. 상태는 EnemySimulation, 행동은 EnemySystem
    public class EnemyController : NetworkBehaviour
    {
        public readonly EnemySimulation Simulation = new();
        public readonly RewindTarget Rewind = new();
        public Health Health => _health;

        [SerializeField] private SOEnemy _settings;
        [SerializeField] private Health _health;

        private void Awake()
        {
            Simulation.Settings = _settings;
        }

        // EnemySpawner가 Spawn 전에 부른다 (Awake 이후)
        public void InjectGrid(PathGrid grid)
        {
            Debug.Assert(grid.Profile == _settings.Profile, $"{name}: 그리드 프로필과 적 프로필이 다름", this);
            Simulation.Grid = grid;
        }

        protected override void OnNetworkPostSpawn()
        {
            if (!IsServer) return;   // 클라는 EnemyRemote가 맡는다

            // TODO: Object Pool에서 나오면 초기화
            EnemySimulation sim = Simulation;
            sim.Movement = new MovementState { Position = transform.position };
            sim.Rotation = transform.rotation;
            sim.MoveInput = Vector2.zero;
            sim.MoveSpeed = 0.0f;
            sim.Path.Clear();
            sim.Waypoint = 0;
            sim.WanderTimer = 0.0f;
            EnemyFsm.Reset(sim);

            Rewind.History.Clear();
            Rewind.Radius = _settings.Profile.Radius;
            Rewind.Height = _settings.Profile.Height;
            
            ServerSimulation.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && ServerSimulation.Instance != null)
            {
                ServerSimulation.Instance.Unregister(this);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            EnemySimulation sim = Simulation;
            string target = sim.TargetClientId.HasValue ? sim.TargetClientId.Value.ToString() : "-";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.0f, $"{sim.StateId}  target:{target}");

            if (_settings != null)
            {
                DrawCapsule();
                DrawRange(_settings.BattleRange, new Color(0.2f, 0.4f, 1.0f));
                DrawRange(_settings.PerceptionRange, new Color(1.0f, 0.85f, 0.2f));
                DrawRange(_settings.AttackRange, new Color(1.0f, 0.25f, 0.25f));
            }

            if (sim.Grid == null || sim.Waypoint >= sim.Path.Count)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = sim.Waypoint; i < sim.Path.Count; i++)
            {
                Vector3 point = sim.Grid.ConvertWorldCoord(sim.Path[i]) + Vector3.up * 0.2f;
                Gizmos.DrawSphere(point, 0.08f);

                Vector3 previous = i == sim.Waypoint ? transform.position : sim.Grid.ConvertWorldCoord(sim.Path[i - 1]) + Vector3.up * 0.2f;
                Gizmos.DrawLine(previous, point);
            }

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            DrawCircle(transform.position, EnemyNavigation.ARRIVE_THRESHOLD);
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
                Gizmos.color = Simulation.Movement.IsGrounded ? Color.yellow : Color.gray;
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