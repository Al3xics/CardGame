using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Playmode;

namespace Wendogo
{
    public class AutoSessionBootstrapper : MonoBehaviour
    {
        [Header("Choose Mode")] [SerializeField]
        private bool autoConnect = false;
        public static bool AutoConnect { get; private set; }
        [Tooltip("Destroy those objects if you don't auto connect. It would mean you pass by the Menu scene, and don't need those objects in this case.")]
        [SerializeField] private List<GameObject> objectsToDestroy = new();
        
        [Space(15)]
        
        [Header("Auto Connection Parameters")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private float retryDelay = 1.0f;
        [SerializeField] private int maxRetries = 10;
        [SerializeField] private int serverPort = 7777;
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private int expectedPlayersCount = 2;
        [SerializeField] private GameObject playerPrefab;

        private void Awake()
        {
            AutoConnect = autoConnect;
            if (AutoConnect) networkManager.OnClientConnectedCallback += OnClientConnected;
        }

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
                    ServerManager.Instance.InitializePlayers();
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

        private void OnClientConnected(ulong clientId)
        {
            // Sur le host uniquement
            if (!networkManager.IsHost) return;

            int connectedClients = networkManager.ConnectedClients.Count;
            Debug.Log($"[Bootstrap] Players connected. Total: {connectedClients} / {expectedPlayersCount}");

            // expectedClientCount + 1 (le host lui-même)
            if (connectedClients >= expectedPlayersCount)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                Debug.Log("[Bootstrap] Every Player is connected. Starting game...");
                
                // Disable GameStateMachine for clients because only the host can do things with it
                GameObject gameStateMachineObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "GameStateMachine");

                if (gameStateMachineObject != null)
                    gameStateMachineObject.SetActive(true);
            }
        }

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