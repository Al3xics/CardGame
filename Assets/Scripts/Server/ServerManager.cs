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
        public event Action OnNightConsequencesEnded;
        public string gameSceneName = "Game";
        private Dictionary<ulong, PlayerController> _playersById;

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (AutoSessionBootstrapper.AutoConnect)
                {
                    // Disable GameStateMachine for clients because only the host can do things with it
                    GameObject gameStateMachineObject = GameObject.Find("GameStateMachine");
                    if (!IsServer && gameStateMachineObject)
                        gameStateMachineObject.SetActive(false);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializePlayers()
        {
            _playersById = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToDictionary(p => p.OwnerClientId);

            foreach (var player in _playersById.Values)
            {
                GameStateMachine.Instance.RegisterPlayerID(player.OwnerClientId);
            }

            if (!AutoSessionBootstrapper.AutoConnect)
            {
                // Disable GameStateMachine for clients because only the host can do things with it
                GameObject gameStateMachineObject = GameObject.Find("GameStateMachine");
                if (!IsServer && gameStateMachineObject)
                    gameStateMachineObject.SetActive(false);
            }
        }

        #region RPC

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
        public void SendCardsToPlayersServerRpc(ulong[] target, int[][] intArray, ServerRpcParams rpcParams = default)
        {
            for (int i = 0; i < target.Length; i++)
            {
                ulong id = target[i];
                int[] cards = intArray[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.SendCardsToClientRpc(cards);
                }
            }
            OnDrawCard?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerTurnServerServerRpc(ulong playerId)
        {
            if (_playersById.TryGetValue(playerId, out var player))
            {
                player.StartMyTurnClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerTurnEndedServerRpc()
        {
            OnPlayerTurnEnded?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransmitMissingCardsServerRpc(int drawXCardsFromDeck, int deckID, ServerRpcParams rpcParams = default)
        {
            ulong idClientAppelant = rpcParams.Receive.SenderClientId;
            GameStateMachine.Instance.DrawCards(idClientAppelant, deckID, drawXCardsFromDeck);
        }

        #endregion
        

        #region Basic Methodes

        public void LaunchGame()
        {
            if (IsServer && SceneManager.GetActiveScene().name != gameSceneName)
                NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        public string GetPlayerName()
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
                return playerName.Value;

            return "Inconnu";
        }

        #endregion


        public void ChangePlayersHealth(Dictionary<ulong,int> playersHealth)
        {

        }

        public void FinishedCheckCardPlayed()
        {

        }

        /*[ServerRpc(RequireOwnership = false)]
        public void TransmitPlayedCardServerRpc(int cardID, ulong target, ServerRpcParams rpcParams = default)
        {
            GameStateMachine.Instance.CheckCardPlayed(cardID, target);
        }*/


        [ServerRpc(RequireOwnership = false)]
        public void SendDataServerServerRpc(ServerRpcParams rpcParams = default)
        {
            OnPlayerTurnEnded?.Invoke();
        }

    }
}