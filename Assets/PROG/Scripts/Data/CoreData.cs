namespace Wendogo
{
    /// <summary>
    /// Enum representing the type of action to perform when setting up a session.
    /// </summary>
    public enum SessionAction
    {
        Invalid,        // Invalid action (default fallback).
        JoinByCode,     // Join a session with a code.
        Create          // Create a new multiplayer session.
    }
    
    /// <summary>
    /// Struct that holds the data required to enter a session (either joining or creating).
    /// </summary>
    public struct EnterSessionData
    {
        /// <summary>
        /// Multiplayer configuration data (such as max players, visibility, network type, etc.).
        /// </summary>
        public MultiplayerConfiguration MultiplayerConfiguration;
        
        /// <summary>
        /// The action to perform with a session.
        /// </summary>
        public SessionAction SessionAction;
        
        /// <summary>
        /// The name of the session.
        /// </summary>
        public string SessionName;
        
        /// <summary>
        /// The code of the session.
        /// </summary>
        public string JoinCode;
        
        /// <summary>
        /// The name of the player trying to enter the session.
        /// </summary>
        public string PlayerName;
    }
    
    /// <summary>
    /// Represent the cycles existing in one turn.
    /// Alternate between Day - Night
    /// </summary>
    public enum Cycle
    {
        Day,
        Night
    }
    
    /// <summary>
    /// All the different types that can be assigned to each player
    /// </summary>
    public enum RoleType
    {
        Survivor,
        Wendogo
    }
    
}