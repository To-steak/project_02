using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public struct StatePayload : INetworkSerializable
    {
        public PlayerSimulationState State;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref State.Tick);
            serializer.SerializeValue(ref State.Movement.Position);
            serializer.SerializeValue(ref State.Movement.VerticalSpeed);
            serializer.SerializeValue(ref State.Movement.IsGrounded);
            serializer.SerializeValue(ref State.Stamina.Energy);
            serializer.SerializeValue(ref State.Stamina.IsExhausted);
        }
    }
}