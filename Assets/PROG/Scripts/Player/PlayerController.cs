using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.Collections;


namespace Wendogo
{
    public class PlayerController : NetworkBehaviour
    {
        #region Variables

        [HideInInspector] public CardObjectData ActiveCard;
        [SerializeField] public EventSystem _inputEvent;
        [SerializeField] public HandManager _handManager;

        [SerializeField] private PlayerUI playerUIInstance;

        private bool uiInitialized = false;

        Dictionary<Button, PlayerController> playerTargets = new Dictionary<Button, PlayerController>();

        List<ulong> playerList = new List<ulong>();

        GameObject selectTargetCanvas;

        public int _playerPA;

        private UniTask _waitForTargetTask = default;

        private int _selectedDeck = -1;
        public int deckID;

        public bool TargetSelected;
        private ulong _selectedTarget = 0;
        private int _intTarget = -1;
        private int _intFood= -1;
        private int _intWood= -1;
        public CardDatabaseSO CardDatabaseSO;

        GameObject _pcSMObject;

        public static PlayerController LocalPlayer;
        public /*static*/ ulong LocalPlayerId;

        private GameObject pcSMObject;

        private int cardDataID;

        public int temporaryTask = -1;
        private GameObject _prefabUI;
        Action<int,int> playerAction;

        #endregion

        #region Network Variables

        public int hiddenHealth;
        public int maxHealth = 8;
        public NetworkVariable<int> health = new(
            8,
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

        public NetworkVariable<RoleType> Role = new(
            value: RoleType.Survivor,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );

        public NetworkVariable<FixedString128Bytes> SessionPlayerId = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
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
        
        public NetworkVariable<bool> hasGardian = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        public PlayerController gardian;
        
        public NetworkVariable<bool> eatPorc = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public bool IsSimulatingNight => ServerManager.Instance.currentCycle.Value == Cycle.Night && IsLocalPlayer;

        #endregion

        #region Action
        
        public static event Action OnCardUsed;
        
        public virtual event Action OnTargetDetection;

        public event Action OnFinishedCardPlayed;

        #endregion

        #region Animation

        private GameObject _popup;
        
        private TMP_Text _popupText;

        #endregion

        #region Basic Method
        
        private void Start()
        {
            name = IsLocalPlayer ? "LocalPlayer" : $"Player{OwnerClientId}";

            if (AutoSessionBootstrapper.AutoConnect)
            {
                // Reference to Pop-up
                _popup = GameObject.FindWithTag("Pop-up");
                if (_popup ==null) throw new Exception("Pop-up not found");
                _popupText = _popup.GetComponentInChildren<TMP_Text>();
            }
        }

        public override void OnNetworkSpawn()
        {
            if (AutoSessionBootstrapper.AutoConnect)
            {
                _inputEvent = GameObject.Find("EventSystem")?.GetComponent<EventSystem>();
                if (_inputEvent != null && _inputEvent.enabled) _inputEvent.enabled = false;
                if (_handManager == null) _handManager = GameObject.FindWithTag("hand")?.GetComponent<HandManager>();
                if (IsOwner)
                {
                    pcSMObject = new GameObject($"{nameof(PlayerControllerSM)}");
                    pcSMObject.AddComponent<PlayerControllerSM>();
                }

                if (_prefabUI == null) { _prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject; }
            }
            if (!IsOwner) return;

            LocalPlayer = this;
            LocalPlayerId = NetworkManager.Singleton.LocalClientId;

            //health.Value = Mathf.Clamp(health.Value, 0, maxHealth);

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
                if (!AutoSessionBootstrapper.AutoConnect)
                {
                    name = SessionManager.Instance.ActiveSession.CurrentPlayer.Properties[SessionConstants.PlayerNamePropertyKey].Value;
                    ServerManager.Instance.RegisterPlayerNameRpc(OwnerClientId, name);
                }
                
                _inputEvent = GameObject.Find("EventSystem")?.GetComponent<EventSystem>();
                if (_inputEvent != null && _inputEvent.enabled) _inputEvent.enabled = false;
                if (_handManager == null) _handManager = GameObject.FindWithTag("hand")?.GetComponent<HandManager>();
                pcSMObject = new GameObject($"{nameof(PlayerControllerSM)}");
                pcSMObject.AddComponent<PlayerControllerSM>();

                // Reference to Pop-up
                _popup = GameObject.FindWithTag("Pop-up");
                if (_popup ==null) throw new Exception("Pop-up not found");
                _popupText = _popup.GetComponentInChildren<TMP_Text>();

                Debug.Log($"This is my player id: {LocalPlayerId}");
                //PlayerUI.Instance.SetPlayerInfos(RpcTarget.Me);

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

        public void DisableInput()
        {
            _inputEvent.enabled = false;

            //Implement enable input
            Debug.Log("Input disabled");
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
            await UniTask.WaitUntil(() => ServerManager.Instance.PlayerReadyCount.Value == 2);
            await UniTask.WaitForSeconds(0.1f);
            EnableInput();
            ServerManager.Instance.ClearVoteRpc();
        }

        public async UniTask GroupSelectTargetVoteAsync()
        {
            _inputEvent.enabled = true;
            _intTarget = -1;

            TargetSelectionUI.OnTargetPicked += HandleTargetSelected;

            await UniTask.WaitUntil(() => _intTarget >= 0);

            TargetSelectionUI.OnTargetPicked -= HandleTargetSelected;

            Debug.Log($"Voted against target is {_intTarget} ");

            _inputEvent.enabled = false;

            ServerManager.Instance.SendVoteRpc(_intTarget);

            Debug.Log($"Waiting for group vote to end");

            //todo change the value to the number of players in the session
            await UniTask.WaitUntil(() => ServerManager.Instance.PlayerReadyCount.Value == 2);

            Debug.Log($"Vote ended");

            ServerManager.Instance.FinishedVotingStateRpc();
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

            Debug.Log("Card selected");
        }

        public void DeselectCard(CardObjectData card)
        {
            //Implement deselect card
            Debug.Log("Card deselected");

            //TweeningManager.CardDown(card.gameObject.transform);
            card.isSelected = false;
        }

        //public async void SelectTarget()
        //{
            //todo move the method in the select state here
        //}

        public void BurnCard()
        {
            //Implement burn card
            if (ActiveCard == null)
                return;

            Debug.Log("Card burnt");

            HandleUsedCard();
            //Placeholder for sending card lacking to server        
            //NotifyMissingCards();
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

        public bool HasEnoughPA()
        {
            return _playerPA > 0;
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

        private void HandlePassiveCardTurnUpdate()
        {
            var passiveCardsList = IsSimulatingNight ? HiddenPassiveCards : PassiveCards;
            var itemsToRemove = new List<(int cardId, GameObject cardObject)>();
            Debug.Log($"$$$$$ [PlayerController] ServerManager CurrentCycle : {ServerManager.Instance.currentCycle.Value.ToString()}");
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
        
        private async void PlayAndWaitAnimation(Animator animator, string animationName, ulong playerId)
        {
            try
            {
                animator.enabled = true;
                if (playerId == LocalPlayerId)
                {
                    DisableInput();
                    _popupText.text = PopupSentences.Instance.thisPlayerTurnText;
                }
                else
                {
                    string playerName;
                    if (AutoSessionBootstrapper.AutoConnect)
                        playerName = GetPlayer(playerId).name;
                    else
                        playerName = ServerManager.Instance.GetPlayerName(playerId);
                    _popupText.text = PopupSentences.Instance.ReplaceX(PopupSentences.Instance.otherPlayerTurnText, $"{playerName}");
                }
                
                animator.Play(animationName, 0, 0f);

                var clip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == animationName);

                if (clip) await UniTask.WaitForSeconds(clip.length);
                else Debug.LogWarning($"Animation {animationName} not found");
            
                animator.enabled = false;
                if (playerId == LocalPlayerId) EnableInput();
                _popupText.text = "";
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public void ShowCard(int card) {}

        #endregion
        
        #region UI Methods

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
            health.Value = Mathf.Clamp(health.Value + delta, 0, maxHealth);
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

        private async UniTaskVoid HandleVoteUIAsync()
        {
            try
            {
                await GroupSelectTargetVoteAsync();
                ServerManager.Instance.FinishedPlayGroupCardRpc();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
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
        public void ApplyBuildRitualRpc(int playedCardID, ulong origin, int nbFood, int nbWood, RpcParams rpcParams)
        {
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardID);
            var buildRitual = card.CardEffect as BuildRitual;
            if (buildRitual == null) throw new Exception("Card effect is not a BuildRitual, but it should be !");
            
            // Replace -1 by 0
            if (nbFood <= -1) nbFood = 0;
            if (nbWood <= -1) nbWood = 0;
            
            // Check if both resources combined are inferior or equal to the maximum use of this card BuildRitual
            var value = nbFood + nbWood;
            if (value > buildRitual.RitualCost)
                // todo --> normalement ça arrive jamais car Valentin vérifie que le total est correcte avant que le joueur valide
                throw new Exception("The sum of the food and wood resources used by the card is superior to the maximum use of this card BuildRitual !");
            
            if (nbFood > 0) buildRitual.ApplyRitualEffect(origin, ResourceType.Food, nbFood);
            if (nbWood > 0) buildRitual.ApplyRitualEffect(origin, ResourceType.Wood, nbWood);
            
            FinishedCardPlayedRpc(RpcTarget.Me);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void FinishedCardPlayedRpc(RpcParams rpcParams)
        {
            // todo -> pourquoi le 'OnFinishedCardPlayed' de 'PCPlayedCardState' est utilisé mais sert à rien ?
            // la méthode 'CardResolutionOver' du state est en commentaire...
            OnFinishedCardPlayed?.Invoke();
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

        [Rpc(SendTo.SpecifiedInParams)]
        public void CheckPlayerHealthRpc(RpcParams rpcParams)
        {
            if (food.Value >= 1)
            {
                food.Value--;
            }
            else
            {
                ServerManager.Instance.ChangePlayerHealthRpc(-1, LocalPlayerId);
            }
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        public void UseVoteUIRpc(bool setUIActive, bool activePlayerInput, RpcParams rpcParams)
        {
            _prefabUI.SetActive(setUIActive);

            if (activePlayerInput)
                _ = HandleVoteUIAsync(); // Fire-and-forget
            else
                ServerManager.Instance.ClearVoteRpc();
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        public void StartPlayAnimationRpc(bool playAndWait, int animatorName, string animationName, ulong playerId, RpcParams rpcParams)
        {
            Animator animator;
            
            switch (animatorName)
            {
                case 0:
                    animator = _popup.GetComponent<Animator>();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(animatorName), animatorName, null);
            }

            PlayAndWaitAnimation(animator, animationName, playerId);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void RequestHealthChangeRpc(int delta, RpcParams rpcParams)
        {
            ChangeHealth(delta);
        }

        #endregion

        #region Notify

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

            int nbFood = -1;
            int nbWood = -1;
            
            // Needs this player to select a target to play the card against
            if (cardDataSO.HasTarget && !cardDataSO.isGroup)
            {
                if (cardDataSO.CardEffect is BuildRitual)
                {
                    _selectedTarget = LocalPlayerId;
                    nbFood = _intFood;
                    nbWood = _intWood;
                }
                else
                {
                    await UniTask.WaitUntil(() => _intTarget >= 0);
                    _selectedTarget = (ulong)_intTarget;
                }

            }
            else if (cardDataSO.isGroup)
            {
                await UniTask.WaitUntil(() => ServerManager.Instance.PlayerReadyCount.Value == 2);
            }
            ServerManager.Instance.TransmitPlayedCardRpc(cardDataSO.ID, _selectedTarget, nbFood, nbWood);
            Debug.Log($"card {cardDataSO.Name} was sent to server ");
        }
        
        #endregion
    }
}