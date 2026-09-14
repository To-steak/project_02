using Unity.Netcode;
using UnityEngine;

public struct InputPayload : INetworkSerializable
{
    public int Tick;
    public Vector3 Move;
    public bool Run;
    public bool Jump;
    public bool Aim;
    public float Pitch;
    public float Yaw;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref Move);
        serializer.SerializeValue(ref Run);
        serializer.SerializeValue(ref Jump);
        serializer.SerializeValue(ref Aim);
        serializer.SerializeValue(ref Pitch);
        serializer.SerializeValue(ref Yaw);
    }
}