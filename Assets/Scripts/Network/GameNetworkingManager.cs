using Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wendogo.Menu;

namespace Wendogo
{
    public class GameNetworkingManager : NetworkBehaviour
    {
        public static GameNetworkingManager Instance { get; private set; }

        public NetworkVariable<int> readyCount = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner
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
        public void PlayerReadyServerRpc()
        {
            readyCount.Value++;

            if (readyCount.Value >= 2)
            {
                NotifyClientsGameReadyClientRpc();
            }
        }

        [ClientRpc]
        private void NotifyClientsGameReadyClientRpc()
        {
            Debug.Log("Finished !!!!");
        }

        public void LaunchGame()
        {
            if (IsServer)
            {
                NetworkManager.SceneManager.LoadScene(
                    "Night_Day_Mech",
                    LoadSceneMode.Single);
            }
        }

        public string GetPlayerName()
        {
            SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var name);
            return name.Value;
        }
    }
}
