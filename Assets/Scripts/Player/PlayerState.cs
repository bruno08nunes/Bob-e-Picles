
using Unity.Netcode;
using UnityEngine;

public struct PlayerState : INetworkSerializable
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 LookDirection;

    // Último input processado pelo servidor.
    public uint LastProcessedTick;

    public void NetworkSerialize<T>(
        BufferSerializer<T> serializer
    ) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Velocity);
        serializer.SerializeValue(ref LookDirection);
        serializer.SerializeValue(ref LastProcessedTick);
    }
}
