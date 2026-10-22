using Unity.Netcode;
using Project02.Character;

namespace Project02.Player
{
    public struct StatePayload : INetworkSerializable
    {
        public PlayerSimulationState State;
        public sbyte InputBuffer;
        public int AckTick;   // 서버가 빠짐없이 받은 마지막 입력 틱 (클라 축). State.Tick은 복제한 틱도 가지므로 ack로 쓰지 않는다

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
            serializer.SerializeValue(ref AckTick);
        }
    }
}