using Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerAPI
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector3 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Run { get; private set; }
        public bool Jump { get; private set; }
        public bool Aim { get; private set; }
        public bool Attack { get; private set; }

        PlayerAction _action;

        public void Initialize()
        {
            _action = new PlayerAction();
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

            _action.Battle.Aim.performed += OnAim;
            _action.Battle.Aim.canceled += OnAim;

            _action.Battle.Attack.performed += OnAttack;
            _action.Battle.Attack.canceled += OnAttack;
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

            _action.Battle.Aim.performed -= OnAim;
            _action.Battle.Aim.canceled -= OnAim;

            _action.Battle.Attack.performed -= OnAttack;
            _action.Battle.Attack.canceled -= OnAttack;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            Move = new Vector3(input.x, 0f, input.y);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) Jump = true;
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            Run = context.performed;
        }

        private void OnAim(InputAction.CallbackContext context)
        {
            Aim = context.performed;
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            Attack = context.performed;
        }

        public InputPayload Capture(int tick, float pitch, float yaw)
        {
            InputPayload payload = new InputPayload
            {
                Tick = tick,
                Move = Move,
                Run = Run,
                Jump = Jump,
                Pitch = pitch,
                Yaw = yaw
            };
            
            Jump = false;
            return payload;
        }
    }
}