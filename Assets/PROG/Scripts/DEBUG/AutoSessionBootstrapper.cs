using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Playmode;

namespace Wendogo
{
    /// <summary>
    /// Provides functionality to bootstrap and configure automatic session settings
    /// within the application, focusing on enabling or disabling auto-connection
    /// behaviors during game initialization.
    /// </summary>
    public class AutoSessionBootstrapper : MonoBehaviour
    {
        /// <summary>
        /// See <see cref="AutoConnect"/> for this information.
        /// </summary>
        [Header("Choose Mode")] [SerializeField]
        private bool autoConnect = false;

        /// <summary>
        /// Indicates whether the application should automatically attempt to connect to a session
        /// upon startup without user intervention. Typically used for automated testing or
        /// specific runtime configurations that bypass manual session setup.
        /// </summary>
        /// <remarks>
        /// - If enabled, the application will automatically configure and initialize a network
        /// session as a host or client based on the player's predefined tags or conditions.
        /// - When enabled, relevant game objects pertaining to manual session setup can be destroyed
        /// or ignored during startup.
        /// - Used in conjunction with <see cref="NetworkManager"/> and other session management logic.
        /// </remarks>
        /// <value>
        /// A boolean property (true/false). Set to true to enable automatic session connection,
        /// or false to rely on a manual setup.
        /// </value>
        public static bool AutoConnect { get; private set; }
        
        [Tooltip("Destroy those objects if you don't auto connect. It would mean you pass by the Menu scene, and don't need those objects in this case.")]
        [SerializeField] private List<GameObject> objectsToDestroy = new();
        
        [Space(15)]
        
        [Header("Auto Connection Parameters")]
        [Tooltip("The NetworkManager component used to manage the session.")]
        [SerializeField] private NetworkManager networkManager;

        [Tooltip("The delay in seconds between each retry attempt to connect to the server.")]
        [SerializeField] private float retryDelay = 1.0f;

        [Tooltip("The maximum number of times to retry connecting to the server.")]
        [SerializeField] private int maxRetries = 10;

        [Tooltip("The port number used by the server to handle incoming connections.")]
        [SerializeField] private int serverPort = 7777;

        [Tooltip("The address of the server to which the application attempts to connect.\nDefault : 127.0.0.1")]
        [SerializeField] private string serverAddress = "127.0.0.1";

        [Tooltip("The number of players expected to be connected in the multiplayer session.\nIt includes the host in the count.")]
        [SerializeField] private int expectedPlayersCount = 2;
        public static int ExpectedPlayersCount { get; private set; }

        [Tooltip("Reference to the player prefab.")]
        [SerializeField] private GameObject playerPrefab;

        /// <summary>
        /// Initializes the AutoSessionBootstrapper instance, setting up necessary configurations
        /// based on the value of the autoConnect property. Attaches the OnClientConnected event
        /// handler if auto connection is enabled.
        /// </summary>
        private void Awake()
        {
            AutoConnect = autoConnect;
            ExpectedPlayersCount = expectedPlayersCount;
            if (AutoConnect) networkManager.OnClientConnectedCallback += OnClientConnected;
        }

        /// <summary>
        /// Starts the auto session bootstrap process, determining whether to auto-connect as a host or client,
        /// or to clean up unnecessary objects if auto connection is disabled.
        /// Executes specific logic depending on the player's role.
        /// </summary>
        /// <returns>IEnumerator to support coroutine-based asynchronous operations.</returns>
        private IEnumerator Start()
        {
            if (AutoConnect)
            {
                var mppmTag = CurrentPlayer.ReadOnlyTags();
                
                if (mppmTag.Contains("Server") || mppmTag.Contains("Host"))
                {
                    networkManager.StartHost();
                    playerPrefab.GetComponent<PlayerController>().SceneLoaded();
                    Debug.Log("[Bootstrap] Host started.");
                }
                else if (mppmTag.Contains("Client"))
                {
                    Debug.Log("[Bootstrap] Client detected. Waiting for server...");
                    yield return StartCoroutine(WaitForServer());
                
                    networkManager.StartClient();
                    playerPrefab.GetComponent<PlayerController>().SceneLoaded();
                    Debug.Log("[Bootstrap] Client started.");
                }
            }
            else
            {
                foreach (var obj in objectsToDestroy)
                {
                    Destroy(obj);
                }
            }
        }

        /// <summary>
        /// Handles the event when a client successfully connects to the server.
        /// This method is executed only on the host to manage connected clients and
        /// to initiate game state changes once all expected players are connected.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client that has just connected.</param>
        private void OnClientConnected(ulong clientId)
        {
            // Only on the host
            if (!networkManager.IsHost) return;

            int connectedClients = networkManager.ConnectedClients.Count;
            Debug.Log($"[Bootstrap] Players connected. Total: {connectedClients} / {expectedPlayersCount}");

            if (connectedClients >= expectedPlayersCount)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                Debug.Log("[Bootstrap] Every Player is connected. Starting game...");
                
                ServerManager.Instance.InitializePlayers();
                
                // Disable GameStateMachine for clients because only the host can do things with it
                GameObject gameStateMachineObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "GameStateMachine");

                if (gameStateMachineObject != null)
                    gameStateMachineObject.SetActive(true);
            }
        }

        /// <summary>
        /// Waits for the server to become available before initiating further actions.
        /// Continuously checks the server's status for availability at specified intervals
        /// and stops retrying after reaching the maximum number of attempts.
        /// </summary>
        /// <returns>Returns a coroutine enumerator that yields during the retry delay intervals.</returns>
        private IEnumerator WaitForServer()
        {
            int attempts = 0;
            while (attempts < maxRetries)
            {
                if (IsServerAvailable(serverAddress, serverPort))
                {
                    yield break;
                }

                attempts++;
                yield return new WaitForSeconds(retryDelay);
            }
        }

        /// <summary>Checks if a server is available at the specified address and port.</summary>
        /// <param name="address">The IP address or hostname of the server to check.</param>
        /// <param name="port">The port number of the server to check.</param>
        /// <returns>True if the server is available and connected; otherwise, false.</returns>
        private bool IsServerAvailable(string address, int port)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect(address, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(200);
                return success && client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}