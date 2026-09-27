using Unity.Netcode;
using UnityEngine;

public struct InputPayload : INetworkSerializable
{
    public int Tick;
    public Vector2 MoveInput;
    public bool RunInput;
    public bool JumpInput;
    public float YawInput;
    public float PitchInput;
    public bool AttackInput;
    public bool AttackPressed;
    public bool AimInput;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref MoveInput);
        serializer.SerializeValue(ref RunInput);
        serializer.SerializeValue(ref JumpInput);
        serializer.SerializeValue(ref YawInput);
        serializer.SerializeValue(ref PitchInput);
        serializer.SerializeValue(ref AttackInput);
        serializer.SerializeValue(ref AttackPressed);
        serializer.SerializeValue(ref AimInput);
    }
}