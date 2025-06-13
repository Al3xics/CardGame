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
        public event Action OnAllocatedActionPoint;
        public event Action OnCheckedHealth;
        public event Action OnNightConsequencesEnded;
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
        public void AssignRolesToPlayersServerRpc(ulong[] clientIds, RoleType[] roles, ServerRpcParams rpcParams = default)
        {
            for (int i = 0; i < clientIds.Length; i++)
            {
                ulong id = clientIds[i];
                RoleType role = roles[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.SendRoleClientRpc(role);
                }
            }

            OnAssignedRoles?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransmitPlayedCardServerRpc(int cardID, ulong target, ServerRpcParams rpcParams = default)
        {
            GameStateMachine.Instance.CheckCardPlayed(cardID, target);
        }


        [ServerRpc(RequireOwnership = false)]
        public void SendDataServerServerRpc(ServerRpcParams rpcParams = default)
        {
            OnPlayerTurnEnded?.Invoke();
        }


        [ServerRpc(RequireOwnership = false)]
        public void SendCardsToPlayersServerRpc(ulong[] ulongArray, int[][] intArray, ServerRpcParams rpcParams = default)
        {
            for (int i = 0; i < ulongArray.Length; i++)
            {
                ulong id = ulongArray[i];
                int[] cards = intArray[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.SendCardsToClientRpc(cards);
                }
            }
            OnDrawCard?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransmitMissingCardsServerRpc(int cardNumber, int deckID, ServerRpcParams rpcParams = default)
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
            if (IsServer && SceneManager.GetActiveScene().name != "Game")
                NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        public string GetPlayerName()
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
                return playerName.Value;

            return "Inconnu";
        }

        public void ChangePlayersHealth(Dictionary<ulong,int> playersHealth)
        {

        }
        public void PlayerTurn(ulong playerId)
        {

        }

        public void FinishedCheckCardPlayed()
        {

        }

    }
}