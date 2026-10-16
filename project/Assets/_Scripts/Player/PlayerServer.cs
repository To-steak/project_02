using Project02.Server;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // Authoring + RPC 프록시: 등록하고, 받은 입력을 쌓기만 한다. 처리는 ServerPlayerSystem
    public class PlayerServer : NetworkBehaviour
    {
        public PlayerController Controller => _controller;
        public readonly InputQueue Inputs = new();

        [SerializeField] private PlayerController _controller;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            ServerSimulation.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && ServerSimulation.Instance != null)
            {
                ServerSimulation.Instance.Unregister(this);
            }
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void InputRPC(InputBundle bundle)
        {
            if (Inputs.Received.Count >= InputQueue.MAX_RECEIVED) return;
            Inputs.Received.Add(bundle);
        }
    }
}