using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;


namespace Wendogo
{
    public class PlayerController : NetworkBehaviour
    {
        #region Variables

        [HideInInspector] public CardObjectData ActiveCard;
        [SerializeField] public EventSystem _inputEvent;
        [SerializeField] public HandManager _handManager;

        private PlayerUI playerUIInstance;

        private bool uiInitialized = false;

        public NetworkVariable<RoleType> Role = new(
            value: RoleType.Survivor,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
            );

        Dictionary<Button, PlayerController> playerTargets = new Dictionary<Button, PlayerController>();

        List<ulong> playerList = new List<ulong>();

        public ulong target;
        GameObject selectTargetCanvas;

        public int _playerPA;


        private int _selectedDeck = -1;
        public int deckID;

        public static event Action OnCardUsed;

        public bool TargetSelected;
        private ulong _selectedTarget = 0;
        public CardDatabaseSO CardDatabaseSO;

        GameObject _pcSMObject;

        public static PlayerController LocalPlayer;
        public static ulong LocalPlayerId;

        private GameObject pcSMObject;
        #endregion

        #region Health & Food & Wood & Cards

        public int hiddenHealth;
        public NetworkVariable<int> health = new(
            10,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public int hiddenWood;
        public NetworkVariable<int> wood = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public int hiddenFood;
        public NetworkVariable<int> food = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public List<CardDataSO> HiddenPassiveCards { get; set; } = new();
        public List<CardDataSO> PassiveCards { get; set; } = new();

        public bool IsSimulatingNight => ServerManager.GetCycle() == Cycle.Night && IsLocalPlayer;

        #endregion

        #region Basic Method

        private void Start()
        {

        }

        public override void OnNetworkSpawn()
        {
            if (AutoSessionBootstrapper.AutoConnect)
            {
                _inputEvent = GameObject.Find("EventSystem")?.GetComponent<EventSystem>();
                if (_inputEvent != null) _inputEvent.enabled = false;
                if (_handManager == null) _handManager = GameObject.FindWithTag("hand")?.GetComponent<HandManager>();
                if (IsOwner)
                {
                    pcSMObject = new GameObject($"{nameof(PlayerControllerSM)}");
                    pcSMObject.AddComponent<PlayerControllerSM>();
                }
            }
            if (!IsOwner) return;

            LocalPlayer = this;
            LocalPlayerId = NetworkManager.Singleton.LocalClientId;
            SceneManager.sceneLoaded += OnSceneLoaded;
            CardDropZone.OnCardDataDropped += NotifyPlayedCard;
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
                _inputEvent = GameObject.Find("EventSystem")?.GetComponent<EventSystem>();
                if (_inputEvent != null) _inputEvent.enabled = false;
                if (_handManager == null) _handManager = GameObject.FindWithTag("hand")?.GetComponent<HandManager>();
                //pcSMObject = new GameObject($"{nameof(PlayerControllerSM)}");
                //pcSMObject.AddComponent<PlayerControllerSM>();
            }
        }

        public void EnableInput()
        {
            _inputEvent.enabled = true;

            //Implement enable input
            Debug.Log("Input enabled");
        }

        public async UniTask<int> SelectDeckAsync(int missingCards)
        {
            if (!IsOwner)
                return -1;

            _selectedDeck = -1;
            DeckClickHandler.OnDeckClicked += HandleDeckClicked;

            Debug.Log($"You have to draw {missingCards} cards : pick a deck.");


            await UniTask.WaitUntil(() => _selectedDeck >= 0);

            DeckClickHandler.OnDeckClicked -= HandleDeckClicked;


            Debug.Log($"Selected deck is : {_selectedDeck}");

            ServerManager.Instance.TransmitMissingCardsServerRpc(missingCards, _selectedDeck);

            return _selectedDeck;
        }

        public async UniTask<ulong> SelectTargetAsync(ulong target)
        {

            TargetSelectionUI.OnTargetPicked += HandleTargetSelected;

            await UniTask.WaitUntil(() => _selectedTarget > 0);

            TargetSelectionUI.OnTargetPicked -= HandleTargetSelected;

            Debug.Log($"Selected target is {_selectedTarget} ");

            return _selectedTarget;
        }

        private void HandleDeckClicked(int deckId)
        {
            _selectedDeck = deckId;
        }

        private void HandleTargetSelected(ulong targetID)
        { 
            _selectedTarget = targetID; 
        }

        public void SelectCard(CardObjectData card)
        {
            //Implement select card

            if (ActiveCard != null)
                DeselectCard(ActiveCard);

            ActiveCard = card;

            //TweeningManager.CardUp(card.gameObject.transform);
            card.isSelected = true;

            if (ActiveCard.Card.HasTarget)
                SelectTarget();

            Debug.Log("Card selected");
        }

        public void DeselectCard(CardObjectData card)
        {
            //Implement deselect card
            Debug.Log("Card deselected");

            //TweeningManager.CardDown(card.gameObject.transform);
            card.isSelected = false;
        }

        public async void SelectTarget()
        {
            //Implement select target
            _selectedTarget = 0;
            //Target selection
            //Handled the cancel next
            await UniTask.WaitUntil(() => _selectedTarget > 0);
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
            //NotifyMissingCards();
            //CheckPA();
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
            //CheckPA();

        }

        public void HandleUsedCard()
        {

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
                //NotifyMissingCards();
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
            return _playerPA > 0;
        }

        public ulong GetChosenTarget()
        {
            return _selectedTarget;
        }

        public int GetMissingCards()
        {
            return _handManager._maxHandSize - _handManager._handCards.Count;
        }

        public static PlayerController GetPlayer(ulong clientId)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
            {
                return networkClient.PlayerObject.GetComponent<PlayerController>();
            }

            Debug.LogWarning($"PlayerController not found for clientId: {clientId}");
            return null;
        }

        public ulong LaunchPlayerSelection(ulong origin, int value = -1)
        {
            selectTargetCanvas.SetActive(true);
            
            
            target = 0;
            return 0;
        }
        
        public void LaunchResourcesSelection(ulong origin, int value = -1)
        {
            
        }


        #endregion

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
                if (IsOwner)
                {
                    _handManager.DrawCard(DrawnCard);
                }
            }

            //if (!HasEnoughPA())
            //{
            //    NotifyEndTurn();
            //}
        }

        [ClientRpc]
        public void StartMyTurnClientRpc()
        {
            if (IsOwner)
                pcSMObject.GetComponent<PlayerControllerSM>().StartStateMachine();
        }


        [ClientRpc]
        public void TryApplyPassiveClientRpc(int playedCardId, ulong origin)
        {
            bool isApplyPassive = false;
            int value = -1;

            // Get the CardDataSO of the played card
            var copyHiddenCards = new List<CardDataSO>(IsSimulatingNight ? HiddenPassiveCards : PassiveCards);

            foreach (var hiddenCard in copyHiddenCards)
            {
                if (hiddenCard.CardEffect.ApplyPassive(playedCardId, origin, OwnerClientId, out value))
                {
                    if (IsSimulatingNight)
                        HiddenPassiveCards.Remove(hiddenCard);
                    else
                        PassiveCards.Remove(hiddenCard);
                    isApplyPassive = true;
                    break;
                }
            }

            ServerManager.Instance.RespondPassiveResultServerRpc(playedCardId, origin, OwnerClientId, isApplyPassive, value);
        }

        [ClientRpc]
        public void FinishedCheckCardPlayedClientRpc()
        {
            if (!IsOwner) return;
            UniTask.Void(async () =>
            {
                int missing = GetMissingCards();

                await SelectDeckAsync(missing);

            });
        }

        [ClientRpc]
        public void DestructAllTrapsClientRpc()
        {
            if (IsSimulatingNight)
            {
                for (int i = HiddenPassiveCards.Count - 1; i >= 0; i--)
                    if (HiddenPassiveCards[i].CardEffect is Trap)
                        HiddenPassiveCards.RemoveAt(i);
            }
            else
            {
                for (int i = PassiveCards.Count - 1; i >= 0; i--)
                    if (PassiveCards[i].CardEffect is Trap)
                        PassiveCards.RemoveAt(i);
            }
        }
        
        /// <summary>
        /// Updates the hidden health, food, and wood values to match their respective public network variables.
        /// Also replicates the list of public passive cards into the hidden passive cards list.
        /// </summary>
        [ClientRpc]
        public void CopyPublicToHiddenClientRpc()
        {
            hiddenHealth = health.Value;
            hiddenFood = food.Value;
            hiddenWood = wood.Value;

            HiddenPassiveCards = new List<CardDataSO>(PassiveCards);
        }

        /// <summary>
        /// Copies the values of hidden health, food, and wood into their respective public network variables.
        /// Also transfers the list of hidden passive cards to the public passive cards list.
        /// </summary>
        [ClientRpc]
        public void CopyHiddenToPublicClientRpc()
        {
            health.Value = hiddenHealth;
            food.Value = hiddenFood;
            wood.Value = hiddenWood;

            PassiveCards = new List<CardDataSO>(HiddenPassiveCards);
        }


        #endregion

        #region Notify

        public void NotifyMissingCards(int missingCards, int deckID)
        {
            ServerManager.Instance.TransmitMissingCardsServerRpc(GetMissingCards(), deckID);
        }

        public void NotifyEndTurn()
        {
            _inputEvent.enabled = false;
            if (_pcSMObject != null)
            {
                Debug.Log("Destroy the player controller");
                Destroy(_pcSMObject);
                _pcSMObject = null;
            }
            ServerManager.Instance.PlayerTurnEndedServerRpc();

        }

        private void NotifyPlayedCard(CardDataSO cardDataSO)
        {
            if (cardDataSO.isPassive)
            {
                _selectedTarget = LocalPlayerId;
            }

            // Needs this player to select a target to play the card against
            // if (cardDataSO.HasTarget)
            //     _selectedTarget = cardDataSO.Target;

            ServerManager.Instance.TransmitPlayedCardServerRpc(cardDataSO.ID, _selectedTarget);
            Debug.Log($"card {cardDataSO.Name} was sent to server ");

        }
        #endregion
    }
}