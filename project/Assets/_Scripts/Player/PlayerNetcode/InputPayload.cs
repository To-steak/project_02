using Unity.Netcode;
using UnityEngine;

public struct InputPayload : INetworkSerializable
{
    public int Tick;
    public Vector3 Move;
    public Vector2 Look;
    public bool Run;
    public float Pitch;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref Move);
        serializer.SerializeValue(ref Look);
        serializer.SerializeValue(ref Run);
        serializer.SerializeValue(ref Pitch);
    }
}