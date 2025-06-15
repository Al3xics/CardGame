namespace Wendogo
{
    /// <summary>
    /// Variables that are accessible everywhere, anytime, and that don't change.
    /// </summary>
    public static class SessionConstants
    {
        /// <summary>
        /// A constant key used to identify the player's name property within session management contexts.
        /// This key is typically used to access or assign the player's name in session-related operations,
        /// such as retrieving or updating player details within a multiplayer session.
        /// </summary>
        public const string PlayerNamePropertyKey = "PlayerName";

        /// <summary>
        /// Represents the key used to identify and access the player's "ready" state in the session properties.
        /// </summary>
        public const string PlayerReadyPropertyKey = "IsReady";

        /// <summary>
        /// A constant string representing the default host name used in session creation
        /// within the application's multiplayer framework. This value will be assigned
        /// when no custom player name is provided by the host during session initialization.
        /// </summary>
        public const string HostName = "Host";

        /// <summary>
        /// Represents the role of a "Survivor" within the game.
        /// </summary>
        public const string Survivor = "Survivor";

        /// <summary>
        /// Represents the role of a "Wendogo" within the game.
        /// </summary>
        public const string Wendogo = "Wendogo";
    }
}