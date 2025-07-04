using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
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
        
        public event Action<ulong, ulong> OnTargetSelected;
        
        public string gameSceneName = "Game";
        private Dictionary<ulong, PlayerController> _playersById;
        
        public NetworkVariable<int> PlayerReadyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
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
                gameStateMachineObject.GetComponent<GameStateMachine>().StartStateMachine();*/
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
        
        public Dictionary<ulong, PlayerController> GetAllPlayers() => _playersById;

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
        
        [Rpc(SendTo.Server)]
        public void TryApplyPassiveRpc(int playedCardId, ulong origin, ulong target)
        {
            if (_playersById.TryGetValue(target, out var player))
                player.TryApplyPassiveRpc(playedCardId, origin, RpcTarget.Single(target, RpcTargetUse.Temp));
        }
        
        [Rpc(SendTo.Server)]
        public void FinishedPassiveCardPlayedRpc(int cardId, ulong origin, bool isHiddenPassiveCards)
        {
            if (_playersById.TryGetValue(origin, out var player))
            {
                if (isHiddenPassiveCards)
                    player.AddHiddenPassiveCardRpc(cardId, RpcTarget.Single(origin, RpcTargetUse.Temp));
                else
                    player.AddPassiveCardRpc(cardId, RpcTarget.Single(origin, RpcTargetUse.Temp));
                
                player.FinishedCardPlayedRpc(RpcTarget.Single(origin, RpcTargetUse.Temp));
            }
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

        [Rpc(SendTo.Server)]
        public void OpenAllUIForVotersRpc(GameObject prefabUI)
        {
            var players = ServerManager.Instance.GetAllPlayers();
            foreach (var player in players.Values)
            {
                if (prefabUI != null)
                {
                    player.OpenVoteUI(prefabUI);
                }
            }
        }
        

        #endregion
    }
}
