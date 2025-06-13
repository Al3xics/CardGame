using System.Collections;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Multiplayer.Playmode;
using System.Threading.Tasks;

namespace Wendogo
{
    public class AutoSessionBootstrapper : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private float retryDelay = 1.0f;
        [SerializeField] private int maxRetries = 10;
        [SerializeField] private int serverPort = 7777;
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private int expectedClientCount = 1;

        private void Awake()
        {
            networkManager.OnClientConnectedCallback += OnClientConnected;
        }

        private IEnumerator Start()
        {
            var mppmTag = CurrentPlayer.ReadOnlyTags();

            if (mppmTag.Contains("Server") || mppmTag.Contains("Host"))
            {
                networkManager.StartHost();
                Debug.Log("[Bootstrap] Host started.");
                ServerManager.Instance.InitializePlayers();
                // Ne pas lancer StartGame ici
            }
            else if (mppmTag.Contains("Client"))
            {
                Debug.Log("[Bootstrap] Client detected. Waiting for server...");
                yield return StartCoroutine(WaitForServer());

                networkManager.StartClient();
                Debug.Log("[Bootstrap] Client started.");
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            // Sur le host uniquement
            if (!networkManager.IsHost) return;

            int connectedClients = networkManager.ConnectedClients.Count;
            Debug.Log($"[Bootstrap] Client connecté. Total: {connectedClients}");

            // expectedClientCount + 1 (le host lui-même)
            if (connectedClients >= expectedClientCount + 1)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                Debug.Log("[Bootstrap] Tous les clients connectés. Lancement du jeu.");
                ServerManager.Instance.StartGame();
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