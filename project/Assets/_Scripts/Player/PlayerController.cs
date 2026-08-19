using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // State
    IPlayerState state;
    PlayerIdleState idleState;
    // API
    PlayerInputs inputs;
    PlayerAnimations animations;
    PlayerMovements movements;

    void Awake()
    {
        // GetComponent
        inputs = GetComponent<PlayerInputs>();
        animations = GetComponent<PlayerAnimations>();
        movements = GetComponent<PlayerMovements>();

        // Player States
        idleState = new PlayerIdleState();
    }

    void OnEnable()
    {

    }

    void Start()
    {
        state = idleState;
    }

    void Update()
    {
        state.Tick();
    }
}
