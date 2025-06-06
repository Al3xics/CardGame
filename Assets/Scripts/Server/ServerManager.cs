using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wendogo
{
    public class ServerManager : NetworkBehaviour
    {
        #region Variables

        public static ServerManager Instance { get; private set; }

        public event Action OnAssignedRoles;
        public event Action OnDrawCard;
        public event Action OnPlayerTurnEnded;
        private Dictionary<ulong, PlayerController> _playersById;

        public NetworkVariable<int> readyCount = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );

        #endregion

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

        public override void OnNetworkSpawn()
        {
            _playersById = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToDictionary(p => p.OwnerClientId);

            foreach (var player in _playersById.Values)
            {
                GameStateMachine.Instance.RegisterPlayerID(player.OwnerClientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AssignRolesToPlayers(Dictionary<ulong, RoleType> playerRoles, ServerRpcParams rpcParams = default)
        {
            foreach (var id in playerRoles.Keys)
            {
                foreach (var player in _playersById.Values)
                {
                    if (player.OwnerClientId == id)
                    {
                        player.SendRoleClientRpc(playerRoles[id]);
                    }
                }
            }
            OnAssignedRoles?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransmitPlayedCard(int cardID, ulong target, ServerRpcParams rpcParams = default)
        {
            GameStateMachine.Instance.CheckCardPlayed(cardID, target);
        }


        [ServerRpc(RequireOwnership = false)]
        public void SendData(ServerRpcParams rpcParams = default)
        {
            foreach (var player in _playersById.Values)
            {
                player.SendRoleClientRpc();
            }

            OnPlayerTurnEnded?.Invoke();
        }


        [ServerRpc(RequireOwnership = false)]
        public void SendCardsToPlayer(Dictionary<ulong, List<int>> playersCards, ServerRpcParams rpcParams = default)
        {
            foreach (var kvp in playersCards)
            {
                if (_playersById.TryGetValue(kvp.Key, out var player))
                {
                    player.SendCardsToClientRpc(kvp.Value);
                }
            }

            OnDrawCard?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransmitMissingCards(int cardNumber, int deckID, ServerRpcParams rpcParams = default)
        {
            ulong idClientAppelant = rpcParams.Receive.SenderClientId;
            GameStateMachine.Instance.DrawCards(idClientAppelant, deckID, cardNumber);
        }


        [ServerRpc(RequireOwnership = false)]
        public void PlayerReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            readyCount.Value++;

            Debug.Log($"Joueur pr�t. Nombre total de joueurs pr�ts : {readyCount.Value}");

            if (readyCount.Value >= 2)
            {
                foreach (var player in _playersById.Values)
                {
                    player.NotifyGameReadyClientRpc();
                }

                LaunchGame();
            }
        }

        public void LaunchGame()
        {
            if (IsServer && SceneManager.GetActiveScene().name != "Night_Day_Mech")
                NetworkManager.SceneManager.LoadScene("Night_Day_Mech", LoadSceneMode.Single);
        }

        public string GetPlayerName()
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
                return playerName.Value;

            return "Inconnu";
        }
    }
}