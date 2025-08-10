namespace Wendogo
{
    /// <summary>
    /// Structure containing all the data when a player performs his action at night.
    /// </summary>
    public struct PlayerAction
    {
        public int CardId;
        public int CardPriorityIndex;
        public ulong OriginId;
        public ulong TargetId;

        public CardDataSO GetCardDataSO() => DataCollection.Instance.cardDatabase.GetCardByID(CardId);
    }
}