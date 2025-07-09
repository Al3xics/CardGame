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

        public string playerNameAsked;
        
        public int playerHealthAsked;
        
        public int playerFoodAsked;
        
        public int playerWoodAsked;
        
        #endregion
        
        #region Network Variables
        
        public NetworkVariable<int> PlayerReadyCount = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private NetworkVariable<int> _playerFinishSceneLoadedCpt = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkList<int> Votes = new(
            new List<int>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        public NetworkVariable<Cycle> CurrentCycle = new(
            Cycle.Day,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
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
            _playerFinishSceneLoadedCpt.Value = 0;
            _playersById = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToDictionary(p => p.OwnerClientId);

            foreach (var player in _playersById.Values)
            {
                GameStateMachine.Instance.RegisterPlayerID(player.OwnerClientId);
            }
        }
        
        [Rpc(SendTo.Server)]
        public void IncrementPlayerFinishedLoadCountRpc()
        {
            _playerFinishSceneLoadedCpt.Value++;

            if (!AutoSessionBootstrapper.AutoConnect)
                if (_playerFinishSceneLoadedCpt.Value >= NetworkManager.Singleton.ConnectedClientsList.Count)
                {
                    Debug.Log("All Players are here.");
                    GameStateMachine.Instance.StartStateMachine();
                }
                else
                {
                    Debug.Log("Waiting for all players to load the scene");
                    Debug.Log($"_playerFinishSceneLoadedCpt = {_playerFinishSceneLoadedCpt}");
                }
        }


        // Starts loading the game scene from the server if it's not already active.
        public void LaunchGame()
        {
            if (IsServer && SceneManager.GetActiveScene().name != gameSceneName)
                NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        public void UpdateCycle(Cycle newCycle)
        {
            if (IsServer) CurrentCycle.Value = newCycle;
        }

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
        
        [Rpc(SendTo.Server)]
        public void GetPlayerNameRpc(ulong clientId)
        {
            if (_playersById.TryGetValue(clientId, out var pc))
            {
                // pull the string ID out of the PC�fs NetworkVariable
                var sessionId = pc.SessionPlayerId.Value.ToString();

                // find that player in the Unity Services session
                var sessionPlayer = SessionManager.Instance.ActiveSession.Players
                    .FirstOrDefault(p => p.Id == sessionId);

                if (sessionPlayer != null
                 && sessionPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var nm))
                {
                    playerNameAsked = nm.Value;
                    return;
                }
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

        [Rpc(SendTo.Server)]
        public void AddRessourceToRitualRpc(bool isHiddenList, bool isFood, bool isRealResource)
        {
            var resourceType = isFood ? ResourceType.Food : ResourceType.Wood;
            GameStateMachine.Instance.AddRessourceToRitual(isHiddenList, resourceType, isRealResource);
        }

        [Rpc(SendTo.Server)]
        public void CheckPlayerHealthRpc()
        {
            foreach (var player in _playersById.Values)
            {
                player.CheckPlayerHealthRpc();
            }
        }

        #endregion
    }
}
