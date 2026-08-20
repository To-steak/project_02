using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public Vector3 Move { get; private set; }

    PlayerActions actions;

    public void Initialize()
    {
        actions = new PlayerActions();
    }

    public void ActiveInputs()
    {
        actions.Enable();
        actions.Battle.Move.performed += OnMove;
        actions.Battle.Move.canceled += OnMove;
        actions.Battle.Jump.performed += OnJump;
    }

    public void DeactiveInputs()
    {
        actions.Disable();
        actions.Battle.Move.performed -= OnMove;
        actions.Battle.Move.canceled -= OnMove;
        actions.Battle.Jump.performed -= OnJump;
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
