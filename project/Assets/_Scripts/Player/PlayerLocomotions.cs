using UnityEngine;

public class PlayerLocomotions : MonoBehaviour
{
    PlayerController _controller;
    PlayerSettingSO _settings;
    CharacterController _character;
    [SerializeField] Transform groundChecker;
    Vector3 _velocity;
    public bool IsGrounded { get; private set; }

    public void Initialize(PlayerController controller, PlayerSettingSO settings)
    {
        _controller = controller;
        _settings = settings;
        _character = GetComponent<CharacterController>();
    }

    public void Move(float speed)
    {
        CheckGrounded();
        ApplyGravity();

        _character.Move(((_controller.Inputs.Move * speed) + _velocity) * Time.deltaTime);
    }

    private void CheckGrounded()
    {
        IsGrounded = Physics.CheckSphere(groundChecker.position, _settings.GroundCheckRadius, _settings.GroundLayer);

        if (IsGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += _settings.GravityValue * Time.deltaTime;
    }
}
