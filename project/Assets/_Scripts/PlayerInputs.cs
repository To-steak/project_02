using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    PlayerActions actions;

    public void Initialzied()
    {
        actions = new PlayerActions();
    }

    void OnEnable()
    {
        actions.Enable();
        actions.Battle.Move.performed += OnMove;
    }

    void OnDisable()
    {
        actions.Disable();
        actions.Battle.Move.performed -= OnMove;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Debug.Log($"{input}");
    }
}
