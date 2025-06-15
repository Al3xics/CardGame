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
        public string GameSceneName { get; private set; } = "Game";
        private Dictionary<ulong, PlayerController> _playersById;

        #endregion

        // Fonction appelée lors de l'initialisation du script pour configurer l'instance du ServerManager
        // et désactiver certains objets pour les clients (non-serveur).
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // Désactive GameStateMachine pour les Clients car seul celui qui host la partie peut exectuter ce code
                GameObject gameStateMachineObject = GameObject.Find("GameStateMachine");
                if (!IsServer && gameStateMachineObject)
                    gameStateMachineObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Initialise le dictionnaire des joueurs présents dans la scène,
        // en associant leur ID réseau à leur PlayerController respectif.
        // Enregistre aussi chaque joueur dans la GameStateMachine.
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

        // Lance le début du jeu en activant la GameStateMachine (réservé à l'hôte).
        public void StartGame()
        {
            Debug.Log("All players are ready. Starting GameStateMachine...");
            // Désactive GameStateMachine pour les Clients car seul celui qui host la partie peut exectuter ce code
            GameObject gameStateMachineObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "GameStateMachine");

            if (gameStateMachineObject != null)
                gameStateMachineObject.SetActive(true);
        }

        #region RPC


        // Attribue les rôles aux joueurs en fonction de leur ID. Fonction appelée côté serveur.
        // Chaque joueur reçoit son rôle via un appel ClientRpc.
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


        // Envoie les cartes tirées à chaque joueur depuis le serveur.
        // Chaque joueur reçoit ses cartes via un appel ClientRpc.
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


        // Démarre le tour du joueur spécifié en appelant un ClientRpc côté client.
        [ServerRpc(RequireOwnership = false)]
        public void PlayerTurnServerServerRpc(ulong playerId)
        {
            if (_playersById.TryGetValue(playerId, out var player))
            {
                player.StartMyTurnClientRpc();
            }
        }


        // Notifie que le tour d’un joueur est terminé en déclenchant l’événement approprié.
        [ServerRpc(RequireOwnership = false)]
        public void PlayerTurnEndedServerRpc()
        {
            OnPlayerTurnEnded?.Invoke();
        }


        // Demande au serveur de faire piocher des cartes à la StateMachine pour un joueur spécifique à partir d’un deck spécifique.
        // Utilisé si des cartes manquent (ex. : après une utilisation).
        [ServerRpc(RequireOwnership = false)]
        public void TransmitMissingCardsServerRpc(int drawXCardsFromDeck, int deckID, ServerRpcParams rpcParams = default)
        {
            ulong idClientAppelant = rpcParams.Receive.SenderClientId;
            GameStateMachine.Instance.DrawCards(idClientAppelant, deckID, drawXCardsFromDeck);
        }

        #endregion


        #region Basic Methodes


        // Lance le chargement de la scène de jeu depuis le serveur si elle n’est pas déjà active.
        public void LaunchGame()
        {
            if (IsServer && SceneManager.GetActiveScene().name != GameSceneName)
                NetworkManager.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }


        // Récupère et retourne le nom du joueur actuel à partir des propriétés de la session.
        public string GetPlayerName()
        {
            if (SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerName))
                return playerName.Value;

            return "Inconnu";
        }

        #endregion

        // Prévue pour modifier les points de vie des joueurs selon un dictionnaire ID / santé.
        public void ChangePlayersHealth(Dictionary<ulong, int> playersHealth)
        {

        }


        // Fonction vide prévue pour signaler la fin de la vérification d'une carte jouée.
        public void FinishedCheckCardPlayed()
        {

        }

        /*[ServerRpc(RequireOwnership = false)]
        public void TransmitPlayedCardServerRpc(int cardID, ulong target, ServerRpcParams rpcParams = default)
        {
            GameStateMachine.Instance.CheckCardPlayed(cardID, target);
        }*/


        // jsp
        [ServerRpc(RequireOwnership = false)]
        public void SendDataServerServerRpc(ServerRpcParams rpcParams = default)
        {
            OnPlayerTurnEnded?.Invoke();
        }

    }
}