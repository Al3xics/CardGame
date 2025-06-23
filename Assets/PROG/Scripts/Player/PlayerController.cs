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
using UnityEditor.Timeline;

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
        List<CardDataSO> hiddenCards = new List<CardDataSO>();

        public int _playerPA;
        public int deckID;

        public static event Action OnCardUsed;

        public bool TargetSelected;
        private ulong _selectedTarget =1;
        public CardDatabaseSO CardDatabaseSO;

        GameObject _pcSMObject;

        private void Start()
        {
            _playerPA = 2;
        }

        public override void OnNetworkSpawn()
        {
            if (_handManager == null)
                _handManager = GameObject.FindWithTag("hand")?.GetComponent<HandManager>();
            _inputEvent = transform.Find("EventSystem")?.GetComponent<EventSystem>();
            if (!IsOwner) return;


            SceneManager.sceneLoaded += OnSceneLoaded;
            CardDropZone.OnCardDropped += NotifyPlayedCard;
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

            //checkCardsconditions
            //if conditions aren't met: return;

            Debug.Log("Card played");

            //Apply effect
            //Not needed because handled by server?
            //NotifyPlayedCard();

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

        public int GetMissingCards()
        {
            return _handManager._maxHandSize - _handManager._handCards.Count;
        }

        public bool TryApplyPassive(CardEffect attackingEffect, ulong origin, out int value)
        {
            foreach (var hiddenCard in hiddenCards)
            {
                foreach (var effect in hiddenCard.CardType.Effects)
                {
                    if (effect.ApplyPassive(attackingEffect, origin, OwnerClientId, out value))
                    {
                        return true;
                    }
                }
            }

            value = -1;
            return false;
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
            foreach (int card in cardsID) 
            { 
                CardDataSO DrawnCard = CardDatabaseSO.GetCardByID(card);
                _handManager.DrawCard(DrawnCard);
            }
        }

        [ClientRpc]
        public void StartMyTurnClientRpc()
        {
            // start the Player State Machine here
           GameObject pcSMObject = new GameObject($"{nameof(PlayerControllerSM)}");
            pcSMObject.AddComponent<PlayerControllerSM>();
        }

        #endregion

        #region Notify

        public void NotifyMissingCards()
        {
            ServerManager.Instance.TransmitMissingCardsServerRpc(GetMissingCards(), deckID);
        }

        private void NotifyEndTurn()
        {
            if (_pcSMObject != null)
            {
                Destroy(_pcSMObject);
                _pcSMObject = null;
            }
            ServerManager.Instance.PlayerTurnEndedServerRpc();
        }

        public void NotifyPlayedCard(CardDataSO cardDataSO)
        {
            ServerManager.Instance.TransmitPlayedCardServerRpc(cardDataSO.ID, _selectedTarget);
            Debug.Log($"card {cardDataSO.Name} was sent to server ");
        }
        #endregion
    }
}