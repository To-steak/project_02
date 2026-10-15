using Project02.Character;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public MovementState Movement => _movement;
        public float Energy => _stamina.Energy;
        public bool IsRunning { get; private set; }

        [SerializeField] private SOMovement _settings;

        private MovementState _movement;
        private StaminaState _stamina;
        private float _maxEnergy;

        // TODO: 순수 View 함수이다. Owner가 Yaw입력에 따른 즉시 회전을 위함.
        // 위치를 옮길 방법고 고려해야 함.
        public void Rotate(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public void Initialize()
        {
            _movement = new MovementState { Position = transform.position };
        }

        public void SetMaxEnergy(float value)
        {
            _maxEnergy = value;
            _stamina = new StaminaState { Energy = value, IsExhausted = false };
        }

        public StatePayload Capture(int tick) => new StatePayload
        {
            Tick = tick,
            Position = _movement.Position,
            VerticalSpeed = _movement.VerticalSpeed,
            IsGrounded = _movement.IsGrounded,
            Mp = _stamina.Energy,
            IsExhausted = _stamina.IsExhausted
        };

        public void RollbackState(StatePayload payload)
        {
            _movement.Position = payload.Position;
            _movement.VerticalSpeed = payload.VerticalSpeed;
            _movement.IsGrounded = payload.IsGrounded;
            _stamina.Energy = payload.Mp;
            _stamina.IsExhausted = payload.IsExhausted;
            transform.position = _movement.Position;
        }

        public void Simulate(InputPayload payload)
        {
            PlayerMovement.Step(_settings, ref _movement, ref _stamina, payload, _maxEnergy, SimulationClock.DeltaTime, out bool isRunning);
            IsRunning = isRunning;
            transform.SetPositionAndRotation(_movement.Position, Quaternion.Euler(0f, payload.YawInput, 0f));
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_settings != null)
            {
                DrawCapsule();
            }
        }

        private void DrawCapsule()
        {
            float radius = _settings.Profile.Radius;
            Vector3 position = transform.position;
            LayerMask layer = _settings.Profile.GroundLayer | _settings.Profile.ObstacleLayer;

            CharacterPhysics.GetCapsule(position, radius, _settings.Profile.Height, out Vector3 bottom, out Vector3 top);

            bool overlapped = Physics.CheckCapsule(bottom, top, radius, layer, QueryTriggerInteraction.Ignore);
            Gizmos.color = overlapped ? Color.red : Color.blue;

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
#endif
    }
}