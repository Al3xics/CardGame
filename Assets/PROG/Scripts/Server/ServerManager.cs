using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Wendogo
{
    public class ServerManager : NetworkBehaviour
    {
        #region Variables

        public static ServerManager Instance { get; private set; }

        public event Action OnAssignedRoles;
        public event Action OnDrawCard;
        public event Action OnPlayerTurnEnded;
        public event Action OnResolveCardNightConsequences;
        public string gameSceneName = "Game";
        private Dictionary<ulong, PlayerController> _playersById;
        private HashSet<int> resolvedCards = new HashSet<int>();


        #endregion

        #region Basic Method
        
        // Called during script initialization to configure the ServerManager instance
        // and disable certain objects for clients (non-server).
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // Disable GameStateMachine for clients because only the host can run this logic
                GameObject gameStateMachineObject = GameObject.Find("GameStateMachine");
                if (!IsServer && gameStateMachineObject)
                    gameStateMachineObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Initializes the dictionary of players present in the scene,
        // mapping their network ID to their corresponding PlayerController.
        // Also registers each player in the GameStateMachine.
        public void InitializePlayers()
        {
            _playersById = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToDictionary(p => p.OwnerClientId);

            foreach (var player in _playersById.Values)
            {
                GameStateMachine.Instance.RegisterPlayerID(player.OwnerClientId);
            }

            /*// Disable GameStateMachine for clients because only the host can do things with it
            GameObject gameStateMachineObject = GameObject.Find("GameStateMachine");
            if (!IsServer && gameStateMachineObject)
                gameStateMachineObject.SetActive(false);*/
        }

        // Starts the game by activating the GameStateMachine (host only).
        public void StartGame()
        {
            Debug.Log("All players are ready. Starting GameStateMachine...");
            // Disable GameStateMachine for clients because only the host can run this logic
            GameObject gameStateMachineObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "GameStateMachine");

            if (gameStateMachineObject != null)
                gameStateMachineObject.SetActive(true);
        }

        // Starts loading the game scene from the server if it's not already active.
        public void LaunchGame()
        {
            if (IsServer && SceneManager.GetActiveScene().name != gameSceneName)
                NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        // Retrieves and returns the current player's name from the session properties.
        public string GetPlayerName()
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
                return playerName.Value;

            return "Unknown";
        }

        public static Cycle GetCycle() => GameStateMachine.Instance.Cycle;

        #endregion

        #region RPC

        // Assigns roles to players based on their network IDs. Called from the server.
        // Each player receives their role via a ClientRpc.
        [ServerRpc(RequireOwnership = false)]
        public void AssignRolesToPlayersServerRpc(ulong[] clientIds, RoleType[] roles, ServerRpcParams rpcParams = default)
        {
            for (int i = 0; i < clientIds.Length; i++)
            {
                ulong id = clientIds[i];
                RoleType role = roles[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.GetRoleClientRpc(role);
                }
            }

            OnAssignedRoles?.Invoke();
        }

        // Sends drawn cards to each player from the server.
        // Each player receives their cards via a ClientRpc.
        [ServerRpc(RequireOwnership = false)]
        public void SendCardsToPlayersServerRpc(ulong[] target, int[][] intArray, ServerRpcParams rpcParams = default)
        {
            for (int i = 0; i < target.Length; i++)
            {
                ulong id = target[i];
                int[] cards = intArray[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.GetCardsClientRpc(cards);
                }
            }
            OnDrawCard?.Invoke();
        }

        // Starts the turn of the specified player by calling a ClientRpc on the client side.
        [ServerRpc(RequireOwnership = false)]
        public void PlayerTurnServerRpc(ulong playerId)
        {
            if (_playersById.TryGetValue(playerId, out var player))
            {
                player.StartMyTurnClientRpc();
            }
        }

        // Notifies that a player's turn has ended by invoking the corresponding event.
        [ServerRpc(RequireOwnership = false)]
        public void PlayerTurnEndedServerRpc()
        {
            OnPlayerTurnEnded?.Invoke();
        }

        // Asks the server to draw cards for a specific player from a specific deck via the GameStateMachine.
        // Used when cards are missing (e.g., after usage).
        [ServerRpc(RequireOwnership = false)]
        public void TransmitMissingCardsServerRpc(int drawXCardsFromDeck, int deckID, ServerRpcParams rpcParams = default)
        {
            ulong idClientAppelant = rpcParams.Receive.SenderClientId;
            GameStateMachine.Instance.DrawCards(idClientAppelant, deckID, drawXCardsFromDeck);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransmitPlayedCardServerRpc(int cardID, ulong target, ServerRpcParams rpcParams = default)
        {
            ulong origin = rpcParams.Receive.SenderClientId;
            GameStateMachine.Instance.CheckCardPlayed(cardID, origin, target);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void TryApplyPassiveServerRpc(int playedCardId, ulong origin, ulong target)
        {
            if (_playersById.TryGetValue(target, out var player))
                player.TryApplyPassiveClientRpc(playedCardId, origin);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void FinishedCheckCardPlayedServerRpc(ulong playerId, ServerRpcParams rpcParams = default)
        {
            // Signal to the playerId that the GameStateMachine finished applying (and checking) the cards he played.
            // Now, he needs to ask for a card with "ServerManager.TransmitMissingCardsServerRpc".
            if (_playersById.TryGetValue(playerId, out var player))
            {
                player.FinishedCheckCardPlayedClientRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RespondPassiveResultServerRpc(int playedCardId, ulong origin, ulong target, bool isApply, int value)
        {
            if (!resolvedCards.Add(playedCardId)) return; // Ignore repeated calls

            GameStateMachine.Instance.OnPassiveResultReceived(playedCardId, origin, target, isApply, value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SynchronizePlayerValuesServerRpc(bool copyToHidden)
        {
            // Parcourt tous les joueurs et délègue la responsabilité au PlayerController
            foreach (var player in _playersById.Values)
            {
                if (copyToHidden)
                    player.CopyPublicToHiddenClientRpc();
                else
                    player.CopyHiddenToPublicClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AskToDestructTrapsServerRpc()
        {
            ulong[] currentPlayerId = new ulong[] { 0, 1, 2, 3 };
            for (int i = 0; i < currentPlayerId.Length; i++)
            {
                ulong id = currentPlayerId[i];
                if (_playersById.TryGetValue(id, out var player))
                {
                    player.DestructAllTrapsClientRpc();
                }
            }
        }

        #endregion
    }
}
