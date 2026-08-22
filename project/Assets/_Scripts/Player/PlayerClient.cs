using Unity.Netcode;
using UnityEngine;

public class PlayerClient : NetworkBehaviour
{
    PlayerController _controller;

    public void Initialize(PlayerController controller)
    {
        _controller = controller;
    }

    public override void OnNetworkSpawn()
    {
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
