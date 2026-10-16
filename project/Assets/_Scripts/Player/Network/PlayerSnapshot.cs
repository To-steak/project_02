using Project02.Character;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public struct PlayerSnapshot : INetworkSerializable
    {
        public ulong ObjectId;
        public MovementState Movement;
        public float Yaw;
        public float Pitch;
        // Animation
        public Vector2 Move;
        public bool IsRunning;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ObjectId);
            MovementStateSerializer.Serialize(serializer, ref Movement);
            serializer.SerializeValue(ref Yaw);
            serializer.SerializeValue(ref Pitch);
            serializer.SerializeValue(ref Move);
            serializer.SerializeValue(ref IsRunning);
        }
    }
}