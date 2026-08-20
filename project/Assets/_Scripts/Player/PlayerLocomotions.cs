using UnityEngine;

public class PlayerLocomotions : MonoBehaviour
{
    PlayerController _controller;
    PlayerSettingSO _settings;
    CharacterController _character;

    public void Initialize(PlayerController controller, PlayerSettingSO settings)
    {
        _controller = controller;
        _settings = settings;
        _character = GetComponent<CharacterController>();
    }

    public void Move()
    {
        
    }
}
