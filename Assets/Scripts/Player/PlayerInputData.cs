using Unity.Netcode;
using UnityEngine;

public struct PlayerInputData : INetworkSerializable
{
    public Vector2 Direction;
    public Vector2 LookDirection;
    public uint Tick;
    public bool IsAttacking;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref Direction);
        serializer.SerializeValue(ref LookDirection);
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref IsAttacking);
    }
}