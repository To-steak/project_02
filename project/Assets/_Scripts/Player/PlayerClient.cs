using Unity.Netcode;
using UnityEngine;

public class PlayerClient : NetworkBehaviour
{
    PlayerController _controller;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        _controller.Inputs.Initialize();
        _controller.Animations.Initialize();
        _controller.Locomotions.Initialize(_controller, _controller.SettingSO);

        if (IsOwner)
        {
            _controller.Inputs.ActiveInputs();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _controller.Inputs.InactiveInputs();
        }
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            MoveRpc(_controller.Inputs.Move);
        }
    }

    [Rpc(SendTo.Server)]
    void MoveRpc(Vector3 move)
    {
        _controller.Inputs.SetMove(move);
    }
}
