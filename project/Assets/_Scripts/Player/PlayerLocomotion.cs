using Project02.Character;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project02.Player
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public MovementState State => _movement;
        public float Mp => _stamina.Energy;
        public bool IsRunning { get; private set; }

        [SerializeField] private SOMovement _settings;

        private MovementState _movement;
        private StaminaState _stamina;

        public void Rotate(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public void Initialize(float maxEnergy)
        {
            _movement = new MovementState { Position = transform.position };
            _stamina = new StaminaState { Energy = maxEnergy, IsExhausted = false };
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

        public void Simulate(InputPayload payload, float maxMp)
        {
            PlayerMovement.Step(_settings, ref _movement, ref _stamina, payload, maxMp, SimulationClock.DeltaTime, out bool isRunning);
            IsRunning = isRunning;
            transform.SetPositionAndRotation(_movement.Position, Quaternion.Euler(0f, payload.YawInput, 0f));
        }
    }
}