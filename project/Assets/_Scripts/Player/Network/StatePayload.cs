using Unity.Netcode;
using Project02.Character;

namespace Project02.Player
{
    public struct StatePayload : INetworkSerializable
    {
        public PlayerSimulationState State;
        public sbyte InputBuffer;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref State.Tick);

            MovementStateSerializer.Serialize(serializer, ref State.Movement);

            serializer.SerializeValue(ref State.Stamina.Energy);
            serializer.SerializeValue(ref State.Stamina.MaxEnergy);
            serializer.SerializeValue(ref State.Stamina.IsExhausted);
            
            serializer.SerializeValue(ref State.Weapon.EquippedId);
            serializer.SerializeValue(ref State.Weapon.Bloom);
            serializer.SerializeValue(ref State.Weapon.LastFireTick);
            serializer.SerializeValue(ref State.Weapon.NextFireTick);

            serializer.SerializeValue(ref InputBuffer);
        }
    }
}