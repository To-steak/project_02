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

        public void Active()
        {
            _action.Enable();
        }

        public void Inactive()
        {
            _action.Disable();
        }

        public void Enable()
        {
            _action.General.Move.performed += OnMove;
            _action.General.Move.canceled += OnMove;

            _action.General.Look.performed += OnLook;
            _action.General.Look.canceled += OnLook;

            _action.General.Jump.performed += OnJump;

            _action.General.Run.performed += OnRun;
            _action.General.Run.canceled += OnRun;

            _action.General.Aim.performed += OnAim;
            _action.General.Aim.canceled += OnAim;

            _action.General.Attack.performed += OnAttack;
            _action.General.Attack.canceled += OnAttack;
        }

        public void Disable()
        {
            _action.General.Move.performed -= OnMove;
            _action.General.Move.canceled -= OnMove;

            _action.General.Look.performed -= OnLook;
            _action.General.Look.canceled -= OnLook;

            _action.General.Jump.performed -= OnJump;

            _action.General.Run.performed -= OnRun;
            _action.General.Run.canceled -= OnRun;

            _action.General.Aim.performed -= OnAim;
            _action.General.Aim.canceled -= OnAim;

            _action.General.Attack.performed -= OnAttack;
            _action.General.Attack.canceled -= OnAttack;
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