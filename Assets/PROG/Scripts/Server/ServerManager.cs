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
        public event Action OnResolveCardNightConsequences;

        public event Action<ulong, ulong> OnTargetSelected;

        public string gameSceneName = "Game";
        public Dictionary<ulong, PlayerController> _playersById { get; private set;}
        
        public NetworkVariable<int> PlayerReadyCount = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public string playerNameAsked;
        
        public int playerHealthAsked;
        
        public int playerFoodAsked;
        
        public int playerWoodAsked;
        
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

        public static Cycle GetCycle() => GameStateMachine.Instance.Cycle;

        #endregion

        #region RPC

        // Assigns roles to players based on their network IDs. Called from the server.
        // Each player receives their role via a ClientRpc.
        [Rpc(SendTo.Server)]
        public void AssignRolesToPlayersRpc(ulong[] clientIds, RoleType[] roles)
        {
            for (int i = 0; i < clientIds.Length; i++)
            {
                ulong id = clientIds[i];
                RoleType role = roles[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.GetRoleRpc(role, RpcTarget.Single(id, RpcTargetUse.Temp));;
                }
            }

            OnAssignedRoles?.Invoke();
        }

        // Sends drawn cards to each player from the server.
        // Each player receives their cards via a ClientRpc.
        [Rpc(SendTo.Server)]
        public void SendCardsToPlayersRpc(ulong[] target, int[][] intArray)
        {
            for (int i = 0; i < target.Length; i++)
            {
                ulong id = target[i];
                int[] cards = intArray[i];

                if (_playersById.TryGetValue(id, out var player))
                {
                    player.GetCardsRpc(cards, RpcTarget.Single(id, RpcTargetUse.Temp));
                }
            }
            OnDrawCard?.Invoke();
        }

        // Starts the turn of the specified player by calling a ClientRpc on the client side.
        [Rpc(SendTo.Server)]
        public void PlayerTurnRpc(ulong playerId)
        {
            if (_playersById.TryGetValue(playerId, out var player))
            {
                player.StartMyTurnRpc(RpcTarget.Single(playerId, RpcTargetUse.Temp));;
            }
        }

        // Notifies that a player's turn has ended by invoking the corresponding event.
        [Rpc(SendTo.Server)]
        public void PlayerTurnEndedRpc()
        {
            OnPlayerTurnEnded?.Invoke();
        }

        // Asks the server to draw cards for a specific player from a specific deck via the GameStateMachine.
        // Used when cards are missing (e.g., after usage).
        [Rpc(SendTo.Server)]
        public void TransmitMissingCardsRpc(int drawXCardsFromDeck, int deckID, RpcParams rpcParams = default)
        {
            ulong idClientAppelant = rpcParams.Receive.SenderClientId;
            GameStateMachine.Instance.DrawCards(idClientAppelant, deckID, drawXCardsFromDeck);
        }

        [Rpc(SendTo.Server)]
        public void TransmitPlayedCardRpc(int cardID, ulong target, RpcParams rpcParams = default)
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

        [Rpc(SendTo.Server)]
        public void SynchronizePlayerValuesRpc(bool copyToHidden)
        {
            foreach (var player in _playersById.Values)
            {
                if (copyToHidden)
                    player.CopyPublicToHiddenRpc(RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
                else
                    player.CopyHiddenToPublicRpc(RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.Server)]
        public void AskToDestructTrapsRpc()
        {
            foreach (var playerId in _playersById)
            {
                var id = playerId.Key;
                if (_playersById.TryGetValue(id, out var player))
                {
                    player.DestructAllTrapsRpc(RpcTarget.Single(id, RpcTargetUse.Temp));
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void UseAllUIForVotersRpc(bool openOrClose)
        {
            foreach (var player in _playersById.Values)
            {
                player.UseVoteUI(openOrClose);
            }
        }
        
        /*[Rpc(SendTo.Server)]
        public string GetPlayerName(ulong clientID)
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
                return playerName.Value;
        
            return "Unknown";
        }*/
        
        [Rpc(SendTo.Server)]
        public void GetPlayerNameRpc(ulong clientID)
        {
            // On parcourt les joueurs pour trouver celui avec le bon ID
            var player = SessionManager.Instance.ActiveSession.Players
                .FirstOrDefault(p => p.Id == clientID.ToString());

            if (player != null && player.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
            {
                playerNameAsked = playerName.Value;
            }

            playerNameAsked = "Unknown";
        }

        [Rpc(SendTo.Server)]
        public void GetPlayerFoodRpc(ulong clientID)
        {
            var player = PlayerController.GetPlayer(clientID);
            playerFoodAsked = player.food.Value;
        }
        
        [Rpc(SendTo.Server)]
        public void GetPlayerWoodRpc(ulong clientID)
        {
            var player = PlayerController.GetPlayer(clientID);
            playerWoodAsked = player.wood.Value;
        }
        
        [Rpc(SendTo.Server)]
        public void GetPlayerHealthRpc(ulong clientID)
        {
            var player = PlayerController.GetPlayer(clientID);
            playerHealthAsked = player.health.Value;
        }
        #endregion
    }
}
