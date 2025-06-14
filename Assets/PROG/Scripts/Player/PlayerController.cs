using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Globalization;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Wendogo
{
    public class PlayerController : NetworkBehaviour
    {
        [HideInInspector] public CardObjectData ActiveCard;
        [SerializeField] public EventSystem _inputEvent;
        [SerializeField] private HandManager _handManager;

        [Header("UI Prefab")]
        public GameObject playerPanelPrefab;

        private PlayerUI playerUIInstance;
        private bool uiInitialized = false;

        public NetworkVariable<RoleType> Role = new(
            value: RoleType.Survivor,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
            );

        Dictionary<Button, PlayerController> playerTargets = new Dictionary<Button, PlayerController>();

        List<ulong> playerList = new List<ulong>();

        public int _playerPA;
        public int deckID;

        public static event Action OnCardUsed;

        public bool TargetSelected;


        private ulong _selectedTarget;

        private void Start()
        {
            _playerPA = 2;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private new void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsOwner || uiInitialized) return;

            if (scene.name == ServerManager.Instance.gameSceneName)
            {
                GameObject mainCanvas = GameObject.Find("MainCanvas");
                if (mainCanvas == null)
                {
                    Debug.LogError("MainCanvas non trouv? !");
                    return;
                }

                GameObject uiObject = Instantiate(playerPanelPrefab, mainCanvas.transform);
                playerUIInstance = uiObject.GetComponent<PlayerUI>();

                if (playerUIInstance != null)
                {
                    uiInitialized = true;
                }
            }
        }

        public void SceneLoaded()
        {
            if (uiInitialized) 
                return;

            GameObject mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas == null)
            {
                Debug.LogError("MainCanvas non trouv? !");
                return;
            }

            GameObject uiObject = Instantiate(playerPanelPrefab, mainCanvas.transform);
            playerUIInstance = uiObject.GetComponent<PlayerUI>();

            if (playerUIInstance != null)
            {
                uiInitialized = true;
            }
        }

        public void EnableInput()
        {
            _inputEvent.enabled = true;

            //Implement enable input
            Debug.Log("Input enabled");
        }

        public async void SelectCard(CardObjectData card)
        {
            //Implement select card

            if (ActiveCard != null)
                DeselectCard(ActiveCard);

            ActiveCard = card;

            TweeningManager.CardUp(card.gameObject.transform);
            card.isSelected = true;

            if (ActiveCard.Card.HasTarget)
                SelectTarget();

            Debug.Log("Card selected");
        }

        public void DeselectCard(CardObjectData card)
        {
            //Implement deselect card
            Debug.Log("Card deselected");

            TweeningManager.CardDown(card.gameObject.transform);
            card.isSelected = false;
        }

        public async void SelectTarget()
        {
            //Implement select target
            TargetSelected = false;
            //Target selection
            //Handled the cancel next
            await UniTask.WaitUntil(() => TargetSelected);
            Debug.Log("Target selected");

            ConfirmPlay();
        }


        public void BurnCard()
        {
            //Implement burn card
            if (ActiveCard == null)
                return;

            Debug.Log("Card burnt");

            HandleUsedCard();
            //Placeholder for sending card lacking to server        
            NotifyMissingCards();
            CheckPA();
        }

        public void ConfirmPlay()
        {
            //Implement confirm play
            if (ActiveCard == null)
                return;

            //CheckCardsconditions
            //if conditions aren't met: return;

            Debug.Log("Card played");

            //Apply effect
            //Not needed because handled by server?
            NotifyPlayedCard();

            HandleUsedCard();
            CheckPA();

        }

        public void HandleUsedCard()
        {
            //Use _playerPA
            _playerPA--;

            //Remove the card from the hand
            _handManager.Discard(ActiveCard.gameObject);

            if (ActiveCard.Card.isPassive)
            {
                Debug.Log("Passive card placed");
            }

            //Destroy(ActiveCard.gameObject);

        }

        public void CheckPA()
        {
            //Check player PA
            if (_playerPA > 0)
                return;

            else
            {
                //Placeholder for sending card lacking to server        
                NotifyMissingCards();
                Debug.Log("Turn is over");
                //Send informations to server
                NotifyEndTurn();
                //Send IDs of played cards to server
                Debug.Log("Sending to server");
                _inputEvent.enabled = false;
            }
        }

        public bool HasEnoughPA()
        {
            return _playerPA >= 0;
        }

        public ulong GetChosenTarget()
        {
            return _selectedTarget;
        }

        public void NotifyPlayedCard()
        {
            //ServerManager.Instance.TransmitPlayedCard(ActiveCard.Card.ID, _selectedTarget);
        }

        public int GetMissingCards()
        {
            return _handManager._maxHandSize - _handManager._handCards.Count;
        }

        #region RPC

        /* -------------------- RPC -------------------- */
        [ClientRpc]
        public void GetRoleClientRpc(RoleType role, ClientRpcParams clientRpcParams = default)
        {
            playerUIInstance?.GetRole(role.ToString());
        }

        [ClientRpc]
        public void GetCardsClientRpc(int[] cardsID, ClientRpcParams clientRpcParams = default)
        {
            // do things with cards
            // this will receive either the 5 first cards, or when this player's turn end, the drawn cards he needs to complete his hand
            // need to handle both events

            //foreach (int ID in cardsID)
            //{
            //    CardDataSO cardData = _cardDatabase.GetDatabaseCardByID(ID); More actions
            //    GameObject cardObject = new GameObject(cardData.Name);
            //    _cardsHandler.ApplyCardData(cardObject, cardData);

            //}

        }

        [ClientRpc]
        public void StartMyTurnClientRpc()
        {
            // start the Player State Machine here

            // DEBUG
            NotifyEndTurn();
        }

        #endregion

        #region Notify

        public void NotifyMissingCards()
        {
            ServerManager.Instance.TransmitMissingCardsServerRpc(GetMissingCards(), deckID);
        }

        private void NotifyEndTurn()
        {
            ServerManager.Instance.PlayerTurnEndedServerRpc();
        }

        #endregion
    }
}