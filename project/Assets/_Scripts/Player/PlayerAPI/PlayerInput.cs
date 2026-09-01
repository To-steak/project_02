using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerAPI
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector3 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Run { get; private set; }

        PlayerAction actions;

        public void Initialize()
        {
            actions = new PlayerAction();
        }

        public void ActiveInputs()
        {
            actions.Enable();

            actions.Battle.Move.performed += OnMove;
            actions.Battle.Move.canceled += OnMove;

            actions.Battle.Look.performed += OnLook;
            actions.Battle.Look.canceled += OnLook;

            actions.Battle.Jump.performed += OnJump;

            actions.Battle.Run.performed += OnRun;
            actions.Battle.Run.canceled += OnRun;
        }

        public void InactiveInputs()
        {
            actions.Disable();

            actions.Battle.Move.performed -= OnMove;
            actions.Battle.Move.canceled -= OnMove;

            actions.Battle.Look.performed -= OnLook;
            actions.Battle.Look.canceled -= OnLook;

            actions.Battle.Jump.performed -= OnJump;

            actions.Battle.Run.performed -= OnRun;
            actions.Battle.Run.canceled -= OnRun;
        }

        public void SetMove(Vector3 move)
        {
            Move = move;
        }

        public void SetLook(Vector2 look)
        {
            Look = look;
        }

        public void SetRun(bool run)
        {
            Run = run;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            Move = new Vector3(input.x, 0f, input.y);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            Debug.Log(context.performed);
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            Run = context.performed;
        }
    }
}