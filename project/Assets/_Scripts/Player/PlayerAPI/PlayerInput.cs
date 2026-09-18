using UnityEngine;
using UnityEngine.InputSystem;
using PlayerNetcode;

namespace PlayerAPI
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool RunInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool AimInput { get; private set; }
        public bool AttackInput { get; private set; }

        private PlayerAction _action;

        public void Initialize(PlayerAction instance)
        {
            _action = instance;
        }

        public void Enable()
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

        public void Disable()
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

        public void Destroy()
        {
            _action?.Dispose();
            _action = null;
        }

        public InputPayload Capture(int tick, float yaw)
        {
            InputPayload payload = new InputPayload
            {
                Tick = tick,
                Move = MoveInput,
                Run = RunInput,
                Jump = JumpInput,
                Yaw = yaw
            };

            JumpInput = false;
            return payload;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) JumpInput = true;
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            RunInput = context.performed;
        }

        private void OnAim(InputAction.CallbackContext context)
        {
            AimInput = context.performed;
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            AttackInput = context.performed;
        }
    }
}