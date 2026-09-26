using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public struct StatePayload : INetworkSerializable
    {
        public int Tick;
        public Vector3 Position;
        public float VerticalSpeed;
        public bool IsGrounded;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref VerticalSpeed);
            serializer.SerializeValue(ref IsGrounded);
        }
    }
}
