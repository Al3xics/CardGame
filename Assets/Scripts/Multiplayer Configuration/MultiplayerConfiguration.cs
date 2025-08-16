using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// ConnectionType is the type of connection to use when creating a session.
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// Session only, no netcode.
        /// </summary>
        None,
        /// <summary>
        /// A direct connection that requires the IP address and port. Also requires either Netcode for GameObjects or Netcode for Entities.
        /// </summary>
        Direct,
        /// <summary>
        /// A Relay connection via Unity Multiplayer Services. Requires either Netcode for GameObjects or Netcode for Entities.
        /// </summary>
        Relay,
#if NGO_2_AVAILABLE            
        /// <summary>
        /// A connection via Distributed Authority that requires Netcode for GameObjects 2.0.0 or higher and Unity Multiplayer Services.
        /// </summary>
        DistributedAuthority,
#endif
    }
    
    /// <summary>
    /// ConnectionMode is used to differentiate between a local (Listen) or public (Publish) connection when direct
    /// connection as the <see cref="ConnectionType"/> is used.
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// Local connection. This is helpful for testing on the same machine or a local network.
        ///
        /// ListenIp and PublishIp are the same. They default to "127.0.0.1".
        /// </summary>
        Listen,
        
        /// <summary>
        /// Specify the listenIp and publishIp.
        ///
        /// To listen on all interfaces, use 0.0.0.0 as the listenIp and specify the external/public IP address
        /// that clients should sue as the publishIp.
        /// </summary>
        Publish
    }
    
    [CreateAssetMenu(fileName = "MultiplayerConfiguration", menuName = "Multiplayer/MultiplayerConfiguration", order = 0)]
    public class MultiplayerConfiguration : ScriptableObject
    {
        #region Connection Settings

        // ---------- Connection Settings ----------
        [Header("Connection Settings")]
        [Tooltip("By default, use Relay.")]
        public ConnectionType connectionType = ConnectionType.Relay;

        #endregion


        #region Direct Connection Settings

        // ---------- Direct Connection Settings ----------
        [Header("Direct Connection Settings")]
        [Tooltip("ConnectionMode is used to differentiate between a local (Listen) or public (Publish) connection when Direct connection type is used.")]
        public ConnectionMode connectionMode = ConnectionMode.Listen;
        
        [Tooltip("Listen for incoming connection at this address. This is the local IP address that the host should use. " +
                 "To listen on all interfaces, use 0.0.0.0 when using ConnectionMode.Publish.\n" +
                 "Default : 127.0.0.1")]
        public string listenIpAddress = "127.0.0.1";
        
        [Tooltip("Address that clients should use when connecting. This is the external/public IP address that clients " +
                 "should use as the publish IP.\n" +
                 "Default : 127.0.0.1")]
        public string publishIpAddress = "127.0.0.1";
        
        [Tooltip("0 selects a randomly available port on the machine and uses the chosen value as the publish port. " +
                 "If a non-zero value is used, the port number applies to both listen and publish addresses.")]
        public int port = 0;

        #endregion


        #region Session Settings

        // ----------Session Settings ----------
        [Header("Session Settings")]
        [Tooltip("The name of the session to create or join.")]
        public string sessionName = "Session";
        
        [Tooltip("The max number of players (including host) allowed in the session.")]
        public int maxPlayers = 4;
        
        [Tooltip("Determines if a session will be locked if a session is created.\nA locked session does not allow any more players to join.")]
        public bool isLocked = false;
        
        [Tooltip("Determines if a session will be private if a session is created.\nPrivate sessions are not visible in queries and cannot be joined with quick-join.\nThey can still be joined by ID or by Code.")]
        public bool isPrivate = false;
        
        [Tooltip("Automatically join a voice channel for a session when joing the session.")]
        public bool enableVoiceChat = true;

        #endregion
    }
}