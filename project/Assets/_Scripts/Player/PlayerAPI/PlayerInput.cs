using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerAPI
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector3 Move { get; private set; }

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
            actions.Battle.Jump.performed += OnJump;
        }

        public void InactiveInputs()
        {
            actions.Disable();
            actions.Battle.Move.performed -= OnMove;
            actions.Battle.Move.canceled -= OnMove;
            actions.Battle.Jump.performed -= OnJump;
        }

        public void SetMove(Vector3 move)
        {
            Move = move;
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
    }
}