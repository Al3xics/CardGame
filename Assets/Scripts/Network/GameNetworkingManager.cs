using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wendogo.Menu;
using Data;

namespace Wendogo
{
    public class GameNetworkingManager : NetworkBehaviour
    {
        public static GameNetworkingManager Instance { get; private set; }

        public NetworkVariable<int> readyCount = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            readyCount.Value++;

            Debug.Log($"Joueur prêt. Nombre total de joueurs prêts : {readyCount.Value}");

            if (readyCount.Value >= 2)
            {
                foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    player.NotifyGameReadyClientRpc();
                }

                LaunchGame();
            }
        }

        public void LaunchGame()
        {
            if (IsServer && SceneManager.GetActiveScene().name != "Night_Day_Mech")
            {
                NetworkManager.SceneManager.LoadScene("Night_Day_Mech", LoadSceneMode.Single);
            }
        }

        public string GetPlayerName()
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var name))
            {
                return name.Value;
            }

            return "Inconnu";
        }
    }
}