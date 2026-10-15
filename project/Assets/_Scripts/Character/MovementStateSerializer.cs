using Unity.Netcode;

namespace Project02.Character
{
    public static class MovementStateSerializer
    {
        public static void Serialize<T>(BufferSerializer<T> serializer, ref MovementState movement) where T : IReaderWriter
        {
            serializer.SerializeValue(ref movement.Position);
            serializer.SerializeValue(ref movement.VerticalSpeed);
            serializer.SerializeValue(ref movement.IsGrounded);
        }
    }
}