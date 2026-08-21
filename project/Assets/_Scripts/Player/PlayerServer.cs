using Unity.Netcode;
using UnityEngine;

public class PlayerServer : NetworkBehaviour
{
    PlayerController _controller;
    PlayerState _state;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        _state = _controller.Idle;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        _state?.Tick();
        _controller.Locomotions.Move(_state.MoveSpeed);
    }

    public void ChangeState(PlayerState state)
    {
        _state.Exit();
        _state = state;
        _controller.NetworkState.Value = state.Type;
        _state.Enter();
    }
}
