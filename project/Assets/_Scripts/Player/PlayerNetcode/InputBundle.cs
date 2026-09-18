using System;
using Unity.Netcode;

namespace PlayerNetcode
{
    public struct InputBundle : INetworkSerializable
    {
        public const int CAPACITY = 3;
        public byte Count;
        public InputPayload Input0;
        public InputPayload Input1;
        public InputPayload Input2;

        public InputPayload this[int index] => index switch
        {
            0 => Input0,
            1 => Input1,
            2 => Input2,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        public void Set(int index, in InputPayload payload)
        {
            switch (index)
            {
                case 0: Input0 = payload; break;
                case 1: Input1 = payload; break;
                case 2: Input2 = payload; break;
                default: throw new IndexOutOfRangeException(nameof(index));
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Count);

            if (Count > CAPACITY) Count = 0;
            if (Count > 0) serializer.SerializeValue(ref Input0);
            if (Count > 1) serializer.SerializeValue(ref Input1);
            if (Count > 2) serializer.SerializeValue(ref Input2);
        }
    }
}