using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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

        public int PlayerPA;
        public int DeckID;

        public static event Action OnCardUsed;

        public bool TargetSelected;


        private ulong _selectedTarget;


        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            _handManager = GetComponent<HandManager>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private new void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsOwner || uiInitialized) return;

            if (scene.name == "Game")
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
                    playerUIInstance.RenamePlayer(GameNetworkingManager.Instance.GetPlayerName());
                    uiInitialized = true;
                }
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
            NotifyPlayedCard();

            HandleUsedCard();
            CheckPA();

        }

        public void HandleUsedCard()
        {
            //Use _playerPA
            PlayerPA--;

            //Remove the card from the hand
            _handManager.Discard(ActiveCard.gameObject);

            if (ActiveCard.Card.isPassive)
            {
                Debug.Log("Passive card placed");
            }

            Destroy(ActiveCard.gameObject);

        }

        public void CheckPA()
        {
            //Check player PA
            if (PlayerPA > 0)
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
            return PlayerPA >= 0;
        }

        public ulong GetChosenTarget()
        {
            return _selectedTarget;
        }

        public void NotifyPlayedCard()
        {
            ServerManager.Instance.TransmitPlayedCardServerRpc(ActiveCard.Card.ID, _selectedTarget);
        }

        public int GetMissingCards()
        {
            return _handManager._maxHandSize - _handManager._handCards.Count;
        }

        public void NotifyMissingCards()
        {

            ServerManager.Instance.TransmitMissingCardsServerRpc(GetMissingCards(), DeckID);
        }

        public async void NotifyEndTurn()
        {
            ServerManager.Instance.SendDataServerServerRpc();
        }

        //Create card with owner directly here
        [ClientRpc]
        public void NotifyGameReadyClientRpc()
        {
            if (IsOwner && playerUIInstance != null)
            {
                playerUIInstance.EndValidation();
            }
        }

        [ClientRpc]
        public void SendRoleClientRpc(RoleType role, ClientRpcParams clientRpcParams = default)
        {
            playerUIInstance.GetRole(role.ToString());
        }

        [ClientRpc]
        public void SendCardsToClientRpc(int[] role, ClientRpcParams clientRpcParams = default)
        {
            playerUIInstance.GetRole(role.ToString());
        }
    }
}