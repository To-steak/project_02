using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerAPI
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector3 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Run { get; private set; }

        PlayerAction _action;
        PlayerEvent _event;

        public void Initialize(PlayerEvent playerEvent)
        {
            _action = new PlayerAction();
            _event = playerEvent;
        }

        public void ActiveInputs()
        {
            _action.Enable();

            _action.Battle.Move.performed += OnMove;
            _action.Battle.Move.canceled += OnMove;

            _action.Battle.Look.performed += OnLook;
            _action.Battle.Look.canceled += OnLook;

            _action.Battle.Jump.performed += OnJump;

            _action.Battle.Run.performed += OnRun;
            _action.Battle.Run.canceled += OnRun;
        }

        public void InactiveInputs()
        {
            _action.Disable();

            _action.Battle.Move.performed -= OnMove;
            _action.Battle.Move.canceled -= OnMove;

            _action.Battle.Look.performed -= OnLook;
            _action.Battle.Look.canceled -= OnLook;

            _action.Battle.Jump.performed -= OnJump;

            _action.Battle.Run.performed -= OnRun;
            _action.Battle.Run.canceled -= OnRun;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            Move = new Vector3(input.x, 0f, input.y);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            _event.RaiseJump();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            Run = context.performed;
        }

        public InputPayload Capture(int tick, float pitch) => new InputPayload
        {
            Tick = tick,
            Move = Move,
            Look = Look,
            Run = Run,
            Pitch = pitch
        };

        public void Apply(InputPayload payload)
        {
            Move = payload.Move;
            Look = payload.Look;
            Run = payload.Run;
        }
    }
}