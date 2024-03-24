using System;
using Unity.Netcode;
public struct LBCharacterSelectState : INetworkSerializable, IEquatable<LBCharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsLockedIn;

    public LBCharacterSelectState(ulong clientId, int characterId = -1, bool isLockedIn = false)
    {
        ClientId = clientId;
        CharacterId = characterId;
        IsLockedIn = isLockedIn;
    }

    public bool Equals(LBCharacterSelectState other)
    {
        return ClientId == other.ClientId && CharacterId == other.CharacterId && IsLockedIn == other.IsLockedIn;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsLockedIn);
    }
}
