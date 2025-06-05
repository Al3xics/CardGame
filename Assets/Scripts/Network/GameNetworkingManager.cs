using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wendogo.Menu;
using Data;
using System.Data;
using System;

namespace Wendogo
{
    public class GameNetworkingManager : NetworkBehaviour
    {
        public static GameNetworkingManager Instance { get; private set; }

        public event Action OnAssignedRoles;
        public event Action OnDrawCard;

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

        /*public override void OnNetworkSpawn()
        {
            foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            {
                GameStateMachine.Instance.RegisterPlayerID(player.OwnerClientId);
            }
        }*/

        /*[ServerRpc(RequireOwnership = false)]
        public void PlaySimpleCard(int cardID, ulong playerID, ServerRpcParams rpcParams = default)
        {
            GameStateMachine.Inctance.PlayCard();
        }*/

        /*[ServerRpc(RequireOwnership = false)]
        public void AssignRolesToPlayers(Dictionary<ulong, RoleType> playerRoles, ServerRpcParams rpcParams = default)
        {
            foreach (var id in playerRoles.Keys)
            {
                foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    if (player.OwnerClientId == id)
                    {
                        player.SendRoleClientRpc(playerRoles[id]);
                    }
                }
            }
            OnAssignedRoles?.Invoke();
        }*/

        [ServerRpc(RequireOwnership = false)]
        public void SendCardsToPlayer(Dictionary<ulong, List<int>> playersCards, ServerRpcParams rpcParams = default)
        {
            foreach (var id in playersCards.Keys)
            {
                foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    if (player.OwnerClientId == id)
                    {
                        player.SendCardsToClientRpc(playersCards[id]);
                    }
                }
            }
            OnDrawCard?.Invoke();
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

        /*[ServerRpc(RequireOwnership = false)]
        public void AskRoleServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong idClientAppelant = rpcParams.Receive.SenderClientId;

            string role;
            if (idClientAppelant == OwnerClientId)
            {
                role = "Survivor";
            }
            else
            {
                role = "Wendgo";
            }

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { idClientAppelant }
                }
            };
            foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            {
                Debug.Log(player.OwnerClientId);
                if (player.OwnerClientId == idClientAppelant)
                {
                    player.SendRoleClientRpc(role, clientRpcParams);
                    break;
                }
            }
        }*/
    }
}