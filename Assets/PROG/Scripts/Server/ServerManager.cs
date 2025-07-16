using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;
using Object = System.Object;


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
        public event Action OnCheckTriggerVote;

        public event Action<ulong, ulong> OnTargetSelected;

        public string gameSceneName = "Game";

        private Dictionary<ulong, PlayerController> PlayersById { get; set;}
        public static Dictionary<ulong, string> GlobalPlayersByName { get; set; } = new();

        public int playerHealthAsked;

        public int playerFoodAsked;

        public int playerWoodAsked;
        
        #endregion

        #region Network Variables

        public NetworkVariable<int> PlayerReadyCount = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<int> _playerFinishSceneLoadedCpt = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkList<int> Votes = new(
            new List<int>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkVariable<Cycle> currentCycle = new(
            Cycle.Day,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkVariable<int> currentTurn = new(
            0,
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
            PlayersById = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToDictionary(p => p.OwnerClientId);

            foreach (var player in PlayersById.Values)
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
            if (IsServer) currentCycle.Value = newCycle;
        }

        public void UpdateTurn(int turn)
        {
            if (IsServer) currentTurn.Value = turn;
        }
        
        public string GetPlayerName(ulong clientId)
        {
            return GlobalPlayersByName.GetValueOrDefault(clientId, "Unknown Player");
        }
        
        public List<PlayerController> GetAllPlayers()
        {
            // Récupère tous les PlayerController sans tri (plus rapide)
            PlayerController[] foundPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            return new List<PlayerController>(foundPlayers);
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

                if (PlayersById.TryGetValue(id, out var player))
                {
                    player.GetRoleRpc(role, RpcTarget.Single(id, RpcTargetUse.Temp));
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

                if (PlayersById.TryGetValue(id, out var player))
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
            if (PlayersById.TryGetValue(playerId, out var player))
            {
                player.StartMyTurnRpc(RpcTarget.Single(playerId, RpcTargetUse.Temp));
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
            if (PlayersById.TryGetValue(target, out var player))
                player.TryApplyPassiveRpc(playedCardId, origin, RpcTarget.Single(target, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server)]
        public void FinishedPassiveCardPlayedRpc(int cardId, ulong origin, bool isHiddenPassiveCards)
        {
            if (PlayersById.TryGetValue(origin, out var player))
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
            foreach (var player in PlayersById.Values)
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
            foreach (var playerId in PlayersById)
            {
                var id = playerId.Key;
                if (PlayersById.TryGetValue(id, out var player))
                {
                    player.DestructAllTrapsRpc(RpcTarget.Single(id, RpcTargetUse.Temp));
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void UseAllUIForVotersRpc(bool setUIActive, bool activePlayerInput)
        {
            foreach (var player in PlayersById.Values)
            {
                player.UseVoteUIRpc(setUIActive, activePlayerInput, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
            }
        }
        
        [Rpc(SendTo.Server)]
        public void RegisterPlayerNameRpc(ulong clientId, string playerName)
        {
            // Adding a new user
            GlobalPlayersByName[clientId] = playerName;

            // Inform all customers of the network-side change (RPC)
            UpdateGlobalNameListClientRpc(clientId, playerName);
        }
        
        [Rpc(SendTo.Server)]
        public void UnregisterPlayerNameRpc(ulong clientId)
        {
            if (IsServer && GlobalPlayersByName.ContainsKey(clientId))
            {
                GlobalPlayersByName.Remove(clientId);

                // Inform all customers of the network-side change (RPC)
                UpdateGlobalNameListClientRpc(clientId, null);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateGlobalNameListClientRpc(ulong clientId, string playerName)
        {
            if (playerName != null)
            {
                // Adding or updating the client-side name
                GlobalPlayersByName[clientId] = playerName;
            }
            else
            {
                // Client-side player removal
                if (GlobalPlayersByName.ContainsKey(clientId))
                    GlobalPlayersByName.Remove(clientId);
            }
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
            foreach (var player in PlayersById.Values)
                player.CheckPlayerHealthRpc(RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server)]
        public void FinishedPlayGroupCardRpc() // todo
        {
            // todo --> call this method when a group card is finished (all players have chosen a target)
            OnResolveCardNightConsequences?.Invoke();
        }

        [Rpc(SendTo.Server)]
        public void FinishedVotingStateRpc()
        {
            OnCheckTriggerVote?.Invoke();
        }

        [Rpc(SendTo.Server)]
        public void StartPlayAnimationRpc(bool playAndWait, int animatorName, string animationName, ulong playerId)
        {
            foreach (var player in PlayersById.Values)
            {
                player.StartPlayAnimationRpc(playAndWait, animatorName, animationName, playerId, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.Server)]
        public void SendVoteRpc(int chosenTarget)
        {
            PlayerReadyCount.Value++;
            Votes.Add(chosenTarget);
            Debug.Log($"Vote done with {chosenTarget} and player ready count at {PlayerReadyCount.Value}");
        }

        [Rpc(SendTo.Server)]
        public void ClearVoteRpc()
        {
            Debug.LogWarning("Votes Cleared");
            Votes.Clear();
            PlayerReadyCount.Value = 0;
        }

        [Rpc(SendTo.Server)]
        public void ChangePlayerHealthRpc(int damage, ulong playerId)
        {
            var player = PlayerController.GetPlayer(playerId);
            player.RequestHealthChangeRpc(damage, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
        }
        
        public void RevealCardsRpc(ulong playerOwnerClientId, List<GameObject> ints)
        {
            // Logique
        }
        #endregion
    }
}
