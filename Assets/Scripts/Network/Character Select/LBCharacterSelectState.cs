using System;
using Unity.Netcode;
public struct LBCharacterSelectState : INetworkSerializable, IEquatable<LBCharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;

    public LBCharacterSelectState(ulong clientId, int characterId = -1)
    {
        ClientId = clientId;
        CharacterId = characterId;
    }

    public bool Equals(LBCharacterSelectState other)
    {
        return ClientId == other.ClientId && CharacterId == other.CharacterId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
    }
}
