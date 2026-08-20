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
        // base.OnNetworkSpawn();
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        _state = _controller.Idle;
    }

    void Update()
    {
        _state?.Tick();
    }

    public void ChangeState(PlayerState state)
    {
        _state.Exit();
        _state = state;
        _controller.NetworkState.Value = state.Type;
        _state.Enter();
    }
}
