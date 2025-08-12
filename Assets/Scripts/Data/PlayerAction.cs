using System;
using Unity.Netcode;

namespace Wendogo
{
    /// <summary>
    /// Structure containing all the data when a player performs his action at night.
    /// </summary>
    public struct PlayerAction: INetworkSerializable, IEquatable<PlayerAction>
    {
        public int CardId;
        public int CardPriorityIndex;
        public ulong OriginId;
        public ulong TargetId;

        public CardDataSO GetCardDataSO() => DataCollection.Instance.cardDatabase.GetCardByID(CardId);


        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref CardId);
            s.SerializeValue(ref CardPriorityIndex);
            s.SerializeValue(ref OriginId);
            s.SerializeValue(ref TargetId);
        }

        public bool Equals(PlayerAction other) =>
            CardId == other.CardId &&
            CardPriorityIndex == other.CardPriorityIndex &&
            OriginId == other.OriginId &&
            TargetId == other.TargetId;

        public override bool Equals(object obj) =>
            obj is PlayerAction other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(CardId, CardPriorityIndex, OriginId, TargetId);

    }
}