using Unity.Netcode;
using UnityEngine;

public struct InputBundle : INetworkSerializable
{
    public InputPayload[] Inputs;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int count = Inputs?.Length ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            Inputs = new InputPayload[count];
        }

        for (int i = 0; i < count; i++)
        {
            serializer.SerializeValue(ref Inputs[i]);
        }
    }
}
