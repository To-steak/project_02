using Unity.Netcode;
using UnityEngine;

namespace PlayerNetcode
{
    public struct InputPayload : INetworkSerializable
    {
        public int Tick;
        public Vector2 Move;
        public bool Run;
        public bool Jump;
        public float Yaw;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Move);
            serializer.SerializeValue(ref Run);
            serializer.SerializeValue(ref Jump);
            serializer.SerializeValue(ref Yaw);
        }
    }
}