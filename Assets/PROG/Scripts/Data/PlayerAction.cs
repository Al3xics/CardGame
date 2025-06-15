namespace Wendogo
{
    /// <summary>
    /// Structure containing all the data that when a player will perform his action at night.
    /// This allows us to store the data of each played card and play all actions of every player when the day begins.
    /// </summary>
    public struct PlayerAction
    {
        public int CardId;
        public ulong TargetId;
    }
}