using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public PlayerSettingSO SettingSO;

    public PlayerIdleState Idle;
    public PlayerWalkState Walk;

    public PlayerInputs Inputs;
    public PlayerAnimations Animations;
    public PlayerLocomotions Locomotions;

    void Awake()
    {
        if (SettingSO == null)
        {
#if UNITY_EDITOR
            Debug.LogError("PlayerSettings is null in PlayerController.cs");
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        Inputs = GetComponent<PlayerInputs>();
        Animations = GetComponent<PlayerAnimations>();
        Locomotions = GetComponent<PlayerLocomotions>();

        Idle = new PlayerIdleState(this);
        Walk = new PlayerWalkState(this);
    }
}
