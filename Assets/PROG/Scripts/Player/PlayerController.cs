using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using System.Threading.Tasks;
using Unity.Collections;


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

        public NetworkVariable<FixedString128Bytes> SessionPlayerId =
    new NetworkVariable<FixedString128Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

        Dictionary<Button, PlayerController> playerTargets = new Dictionary<Button, PlayerController>();

        List<ulong> playerList = new List<ulong>();

        public ulong target;
        GameObject selectTargetCanvas;

        public int _playerPA;

        private UniTask _waitForTargetTask = default;

        private int _selectedDeck = -1;
        public int deckID;

        public static event Action OnCardUsed;

        public bool TargetSelected;
        private ulong _selectedTarget = 0;
        private int _intTarget = -1;
        public CardDatabaseSO CardDatabaseSO;

        GameObject _pcSMObject;

        public static PlayerController LocalPlayer;
        public static ulong LocalPlayerId;

        private GameObject pcSMObject;

        private int cardDataID;
        
        public virtual event Action OnTargetDetection;
        
        public int temporaryTask = -1;
        private GameObject _prefabUI;

        #endregion

        #region Health & Food & Wood & Cards

        public int hiddenHealth;
        public NetworkVariable<int> health = new(
            6,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public int hiddenWood;
        public NetworkVariable<int> wood = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public int hiddenFood;
        public NetworkVariable<int> food = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkList<int> PassiveCards = new(
            new List<int>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkList<int> HiddenPassiveCards = new(
            new List<int>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public bool IsSimulatingNight => ServerManager.Instance.CurrentCycle.Value == Cycle.Night && IsLocalPlayer;

        #endregion

        #region Basic Method
        
        private void Start()
        {
            name = IsLocalPlayer ? "LocalPlayer" : $"Player{OwnerClientId}";
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

                if (_prefabUI == null) { _prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject; }

                //PlayerUI.Instance.SetPlayerInfos();
            }
            if (!IsOwner) return;

            LocalPlayer = this;
            LocalPlayerId = NetworkManager.Singleton.LocalClientId;

            health.Value = Mathf.Clamp(health.Value, 0, 6);

            food.OnValueChanged += UpdateFoodText;
            wood.OnValueChanged += UpdateWoodText;
            health.OnValueChanged += UpdateHearts;
            SceneManager.sceneLoaded += OnSceneLoaded;
            CardDropZone.OnCardDataDropped += NotifyPlayedCard;
        }

        private new void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            health.OnValueChanged -= UpdateHearts;
            food.OnValueChanged -= UpdateFoodText;
            wood.OnValueChanged -= UpdateWoodText;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            if (!IsOwner || uiInitialized) return;

            if (scene.name == ServerManager.Instance.gameSceneName)
            {
                _inputEvent = GameObject.Find("EventSystem")?.GetComponent<EventSystem>();
                if (_inputEvent != null) _inputEvent.enabled = false;
                if (_handManager == null) _handManager = GameObject.FindWithTag("hand")?.GetComponent<HandManager>();
                pcSMObject = new GameObject($"{nameof(PlayerControllerSM)}");
                pcSMObject.AddComponent<PlayerControllerSM>();

                Debug.Log($"This is my player id: {LocalPlayerId}");
                PlayerUI.Instance.SetPlayerInfos();

                if (_prefabUI == null) { _prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject; }

                ServerManager.Instance.IncrementPlayerFinishedLoadCountRpc();
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

            ServerManager.Instance.TransmitMissingCardsRpc(missingCards, _selectedDeck);

            return _selectedDeck;
        }

        public async UniTask SelectTargetAsync()
        {
            _intTarget = -1;
            TargetSelectionUI.OnTargetPicked += HandleTargetSelected;

            await UniTask.WaitUntil(() => _intTarget >= 0);

            TargetSelectionUI.OnTargetPicked -= HandleTargetSelected;

            Debug.Log($"Selected target is {_intTarget} ");
        }


        public async UniTask GroupSelectTargetAsync()
        {
            _intTarget = -1;
            TargetSelectionUI.OnTargetPicked += HandleTargetSelected;

            await UniTask.WaitUntil(() => _intTarget >= 0);

            ServerManager.Instance.PlayerReadyCount.Value++;

            TargetSelectionUI.OnTargetPicked -= HandleTargetSelected;

            Debug.Log($"Voted against target is {_intTarget} ");

            ServerManager.Instance.Votes.Add(_intTarget);

            Debug.Log($"Waiting for group vote to end");

            await UniTask.WaitUntil(() => ServerManager.Instance.PlayerReadyCount.Value == 4);

            Debug.Log($"Vote ended");
        }

        private void HandleDeckClicked(int deckId)
        {
            _selectedDeck = deckId;
        }

        private void HandleTargetSelected(int targetID)
        {
            _intTarget = targetID;
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
            await UniTask.WaitUntil(() => _intTarget >= 0);
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

            //CheckCardsconditions
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
            else
            {
                Destroy(ActiveCard.gameObject);
            }
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

        public int GetChosenTarget()
        {
            return _intTarget;
        }

        public int GetMissingCards()
        {
            return _handManager._maxHandSize - _handManager.handCards.Count;
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

        private List<int> GetPassiveCardCopy()
        {
            var copy = new List<int>();
            var source = IsSimulatingNight ? HiddenPassiveCards : PassiveCards;

            foreach (var cardId in source)
                copy.Add(cardId);

            return copy;
        }

        public CardDataSO GetCardByID(int cardId)
        {
            return DataCollection.Instance.cardDatabase.GetCardByID(cardId);
        }

        #region UI Methods
        
        public void UseVoteUI(bool openOrClose)
        {
            _prefabUI.SetActive(openOrClose);
        }

        public void UpdateFoodText(int oldFoodValue, int newFoodValue)
        {
            PlayerUI.Instance.DefineFoodText(newFoodValue);
        }

        public void UpdateWoodText(int oldWoodValue, int newWoodValue)
        {
            PlayerUI.Instance.DefineWoodText(newWoodValue);
        }

        public void ChangeHealth(int delta)
        {
            health.Value = Mathf.Clamp(health.Value + delta, 0, 6);
        }

        public void UpdateHearts(int oldHealthValue, int newHealthValue)
        {
            Debug.Log($"New health is: {newHealthValue} and old health is {oldHealthValue} ");
            if (newHealthValue < oldHealthValue)
                for (int i = newHealthValue; i < oldHealthValue; i++)
                {
                    PlayerUI.Instance.hearts[i].gameObject.SetActive(false);
                }
            else if (newHealthValue > oldHealthValue)
                for (int i = oldHealthValue; i < newHealthValue; i++)
                {
                    PlayerUI.Instance.hearts[i].gameObject.SetActive(true);
                }
        }

        #endregion

        private void HandlePassiveCardTurnUpdate()
        {
            var passiveCardsList = IsSimulatingNight ? HiddenPassiveCards : PassiveCards;
            var itemsToRemove = new List<(int cardId, GameObject cardObject)>();
            
            Debug.Log($"$$$$$ [PlayerController] ServerManager CurrentCycle : {ServerManager.Instance.CurrentCycle.Value.ToString()}");
            Debug.Log($"$$$$$ [PlayerController] Passive cards list : {passiveCardsList.Count}, for player {GetPlayer(LocalPlayerId).name} ({GetPlayer(LocalPlayerId).OwnerClientId})");

            // Iterate through all logical passive cards (IDs)
            foreach (var cardId in passiveCardsList)
            {
                // var card = DataCollection.Instance.cardDatabase.GetCardByID(cardId);
                var card = _handManager.GetCardDataInPassiveZone(cardId);
                
                if (card == null || !card.isPassive) continue;
                
                Debug.Log($"$$$$$ [PlayerController] Current turns remaining for passive card {card.Name} : {card.turnsRemaining}");
                
                if (card.turnsRemaining == -1) continue;
                if (card.turnsRemaining > 0) card.turnsRemaining--;
                Debug.Log($"$$$$$ [PlayerController] Turns remaining for passive card {card.Name} : {card.turnsRemaining}");
                if (card.turnsRemaining <= 0)
                {
                    GameObject cardObject = _handManager.GetCardGameObjectInPassiveZone(cardId);
                    itemsToRemove.Add((cardId, cardObject));
                    Debug.Log($"Passive card {card.Name} expired.");
                    break;
                }
            }

            if (itemsToRemove.Count == 0) return;
            
            foreach (var (cardId, cardObject) in itemsToRemove)
            {
                passiveCardsList.Remove(cardId);
                if (cardObject != null) _handManager.RemoveCardFromPassiveZone(cardObject);
                Debug.Log($"Card with ID: {cardId} removed from {(IsSimulatingNight ? "HiddenPassiveCards" : "PassiveCards")}");
            }
            
            Debug.Log($"$$$$$ [PlayerController] passiveCardsList Count : {passiveCardsList.Count}");
            
            if (IsSimulatingNight)
                Debug.Log($"$$$$$ [PlayerController] HiddenPassiveCardsList Count : {HiddenPassiveCards.Count}");
            else
                Debug.Log($"$$$$$ [PlayerController] PassiveCards Count : {PassiveCards.Count}");
                
        }

        #endregion

        #region RPC

        /* -------------------- RPC -------------------- */
        [Rpc(SendTo.SpecifiedInParams)]
        public void GetRoleRpc(RoleType role, RpcParams rpcParams)
        {
            playerUIInstance?.GetRole(role.ToString());
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void GetCardsRpc(int[] cardsID, RpcParams rpcParams)
        {
            foreach (int card in cardsID)
            {
                if (IsOwner)
                {
                    CardDataSO clonedCard = CardDataSO.Clone(CardDatabaseSO.GetCardByID(card));
                    _handManager.DrawCard(clonedCard);
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void StartMyTurnRpc(RpcParams rpcParams)
        {
            if (IsOwner)
                pcSMObject.GetComponent<PlayerControllerSM>().StartStateMachine();
        }


        [Rpc(SendTo.SpecifiedInParams)]
        public void TryApplyPassiveRpc(int playedCardId, ulong origin, RpcParams rpcParams)
        {
            Debug.Log($" Try apply passive on player : {name}");
            bool isApplyPassive = false;
            int value = -1;

            // Get the CardDataSO of the played card
            var cardIds = GetPassiveCardCopy();
            
            foreach (var cardId in cardIds)
            {
                var hiddenCard = DataCollection.Instance.cardDatabase.GetCardByID(cardId);

                if (hiddenCard.CardEffect.ApplyPassive(playedCardId, origin, OwnerClientId, out value))
                {
                    isApplyPassive = true;
                    break;
                }
            }
            
            var effect = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId).CardEffect;
            effect.Apply(origin, OwnerClientId, isApplyPassive ? value : -1);
            FinishedCardPlayedRpc(RpcTarget.Me);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void FinishedCardPlayedRpc(RpcParams rpcParams)
        {

        }

        /// <summary>
        /// Updates the hidden health, food, and wood values to match their respective public network variables.
        /// Also replicates the list of public passive cards into the hidden passive cards list.
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        public void CopyPublicToHiddenRpc(RpcParams rpcParams)
        {
            hiddenHealth = health.Value;
            hiddenFood = food.Value;
            hiddenWood = wood.Value;

            HiddenPassiveCards.Clear();
            foreach (var cardId in PassiveCards)
                HiddenPassiveCards.Add(cardId);
            
            Debug.Log($"[CopyPublicToHiddenRpc] RealHealth : {health.Value} RealFood : {food.Value} RealWood : {wood.Value}");
            Debug.Log($"[CopyPublicToHiddenRpc] PassiveCards : {PassiveCards.Count}");
        }

        /// <summary>
        /// Copies the values of hidden health, food, and wood into their respective public network variables.
        /// Also transfers the list of hidden passive cards to the public passive cards list.
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        public void CopyHiddenToPublicRpc(RpcParams rpcParams)
        {
            health.Value = hiddenHealth;
            food.Value = hiddenFood;
            wood.Value = hiddenWood;

            PassiveCards.Clear();
            foreach (var cardId in HiddenPassiveCards)
                PassiveCards.Add(cardId);
            
            Debug.Log($"[CopyHiddenToPublicRpc] RealHealth : {health.Value} RealFood : {food.Value} RealWood : {wood.Value}");
            Debug.Log($"[CopyPublicToHiddenRpc] PassiveCards : {PassiveCards.Count}");
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        public void DestructAllTrapsRpc(RpcParams rpcParams)
        {
            if (IsSimulatingNight)
            {
                for (int i = HiddenPassiveCards.Count - 1; i >= 0; i--)
                {
                    var card = DataCollection.Instance.cardDatabase.GetCardByID(HiddenPassiveCards[i]);
                    if (card.CardEffect is Trap) HiddenPassiveCards.RemoveAt(i);
                }
            }
            else
            {
                for (int i = PassiveCards.Count - 1; i >= 0; i--)
                {
                    var card = DataCollection.Instance.cardDatabase.GetCardByID(PassiveCards[i]);
                    if (card.CardEffect is Trap) PassiveCards.RemoveAt(i);
                }
            }
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        public void AddPassiveCardRpc(int cardId, RpcParams rpcParams) => PassiveCards.Add(cardId);

        [Rpc(SendTo.SpecifiedInParams)]
        public void AddHiddenPassiveCardRpc(int cardId, RpcParams rpcParams) => HiddenPassiveCards.Add(cardId);

        [Rpc(SendTo.Server)]
        public void RegisterSessionIdServerRpc(string sessionPlayerId)
        {
            SessionPlayerId.Value = sessionPlayerId;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void CheckPlayerHealthRpc(RpcParams rpcParams)
        {
            // todo
        }

        #endregion

        #region Notify

        public void NotifyMissingCards(int missingCards, int deckID)
        {
            ServerManager.Instance.TransmitMissingCardsRpc(GetMissingCards(), deckID);
        }

        public void NotifyEndTurn()
        {
            _inputEvent.enabled = false;
            HandlePassiveCardTurnUpdate();
            if (_pcSMObject != null)
            {
                Debug.Log("Destroy the player controller");
                Destroy(_pcSMObject);
                _pcSMObject = null;
            }
            ServerManager.Instance.PlayerTurnEndedRpc();

        }

        private async void NotifyPlayedCard(CardDataSO cardDataSO)
        {
            if (cardDataSO.isPassive || !cardDataSO.HasTarget)
            {
                _selectedTarget = LocalPlayerId;
            }

            // Needs this player to select a target to play the card against
            if (cardDataSO.HasTarget)
            {
                await UniTask.WaitUntil(() => _intTarget >= 0);
                _selectedTarget = (ulong)_intTarget;

            }

            ServerManager.Instance.TransmitPlayedCardRpc(cardDataSO.ID, _selectedTarget);
            Debug.Log($"card {cardDataSO.Name} was sent to server ");
        }
        #endregion
    }
}