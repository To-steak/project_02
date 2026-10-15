using Project02.Character;
using Unity.Netcode;

namespace Project02.Enemy
{
    public struct EnemySnapshot : INetworkSerializable
    {
        public ulong ObjectId;
        public MovementState Movement;
        public float Yaw;
        public EnemyStateId StateId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ObjectId);
            MovementStateSerializer.Serialize(serializer, ref Movement);
            serializer.SerializeValue(ref Yaw);
            serializer.SerializeValue(ref StateId);
        }
    }
}