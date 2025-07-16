using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents the main state machine responsible for managing the game's flow and transitions between various states.
    /// </summary>
    public class GameStateMachine : StateMachine<GameStateMachine>
    {
        #region Instance
        
        /// <summary>
        /// Gets the singleton instance of the <see cref="GameStateMachine"/>.
        /// This property provides access to the single, globally accessible instance
        /// of the <see cref="GameStateMachine"/> class, ensuring it follows a singleton pattern.
        /// </summary>
        public static GameStateMachine Instance { get; private set; }

        #endregion
        
        #region Variables
        
        /* --------------- Show in Inspector --------------- */
        /// <summary>
        /// Represents the maximum number of turns allowed in the game.
        /// If the number of completed turns reaches this value, the game will end.
        /// </summary>
        [Header("Game Settings")]
        [SerializeField] private int maximumTurn = 10;

        /// <summary>
        /// Represents the number of cards each player receives in the action deck at the start of the game.
        /// </summary>
        public int startingActionDeckAmount = 3;

        /// <summary>
        /// Represents the number of cards each player receives in the resource deck at the start of the game.
        /// </summary>
        public int startingResourceDeckAmount = 2;

        /// <summary>
        /// Represents the number of turns after which a vote is triggered in the game.
        /// </summary>
        public int triggerVoteEveryXTurn = 2;

        /// <summary>
        /// Represents the required number of food items needed to successfully complete the ritual in the game.
        /// This value is used as a condition within the game's mechanics to determine if the ritual goals
        /// have been met during gameplay.
        /// </summary>
        public int numberOfFoodToCompleteRitual = 6;

        /// <summary>
        /// Represents the number of wood pieces required to successfully complete the ritual.
        /// This value serves as a key resource target that players must gather to progress through or achieve
        /// ritual-related goals within the game.
        /// </summary>
        public int numberOfWoodToCompleteRitual = 6;

        /* --------------- Hide in Inspector --------------- */
        /// <summary>
        /// Tracks the current turn count within the game cycle.
        /// </summary>
        private int _cptTurn = 0;

        /// <summary>
        /// Tracks the number of turns since the last vote triggered.
        /// </summary>
        /// <remarks>
        /// This variable is used to determine when a vote should be triggered
        /// based on the specified interval, defined by the triggerVoteEveryXTurn field.
        /// It is incremented after every turn and reset when a vote is required.
        /// </remarks>
        private int _cptTurnForVote = 1;

        /// <summary>
        /// Represents the collection of player actions performed during the night phase in the game. Only for cards that
        /// have a <see cref="CardDataSO.nightPriorityIndex"/> different than <c>0</c>.
        /// This list is used to store and manage actions taken by players, which are then processed
        /// during the night cycle state transitions.
        /// </summary>
        public readonly List<PlayerAction> NightActions = new();
        
        /// <summary>
        /// Represents the ID of the current player whose turn is active in the game.
        /// This variable helps manage the game flow by tracking which player's turn is currently in progress.
        /// It is incremented sequentially to move to the next player in <see cref="PlayerTurnState.OnPlayerTurnEnded"/>.
        /// </summary>
        public int CurrentPlayerId { get; set; } = 0;
        
        /// <summary>
        /// Represents a list containing the unique identifiers (IDs) of all players currently
        /// participating in the game. Used to manage player-specific data and turn orders.
        /// </summary>
        public List<ulong> PlayersID { get; private set; } = new();

        /// <summary>
        /// Represents the current <see cref="Cycle"/> of the game, which can be either Day or Night.
        /// The state of the cycle determines the flow of the game's behavior and logic.
        /// </summary>
        public Cycle Cycle { get; private set; } = Cycle.Day;

        private bool _isRitualOver = false;
        
        /// <summary>
        /// Indicates whether the ritual in the game has been completed or not.
        /// This property is used to control the flow of game states, transitioning
        /// to the end game state if the ritual is complete or proceeding
        /// with the normal game cycle.
        /// </summary>
        public bool IsRitualOver
        {
            get
            {
                if (CheckRitualOver())
                    _isRitualOver = true;
                return _isRitualOver;
            }
            private set => _isRitualOver = value;
        }

        /// <summary>
        /// Represents a collection that tracks the status of collected food for the ritual.
        /// Each entry in the list indicates whether a particular food resource contributes meaningfully
        /// to the ritual completion based on its validity.
        /// </summary>
        private readonly List<bool> _ritualFoodCollected = new();

        /// <summary>
        /// Represents a private collection tracking the hidden food contributions by players
        /// during the <see cref="Cycle.Night"/> cycle. See <see cref="_ritualFoodCollected"/> for the ritual.
        /// </summary>
        private readonly List<bool> _hiddenRitualFoodCollected = new();

        /// <summary>
        /// Represents a collection that tracks the status of collected wood for the ritual.
        /// Each entry in the list indicates whether a particular wood resource contributes meaningfully
        /// to the ritual completion based on its validity.
        /// </summary>
        private readonly List<bool> _ritualWoodCollected = new();

        /// <summary>
        /// Represents a private collection tracking the hidden wood contributions by players
        /// during the <see cref="Cycle.Night"/> cycle. See <see cref="_ritualWoodCollected"/> for the ritual.
        /// </summary>
        private readonly List<bool> _hiddenRitualWoodCollected = new();

        #endregion

        #region Basic Methods

        /// <summary>
        /// Initializes the GameStateMachine instance, ensuring that only one instance exists and
        /// initializing players if auto-connection is disabled.
        /// </summary>
        private void Awake()
        {
            if (!Instance)
                Instance = this;
            
            if (!AutoSessionBootstrapper.AutoConnect)
                ServerManager.Instance.InitializePlayers();
        }

        /// <summary>
        /// Gets the initial state for the game state machine.
        /// This state will be used to start the state machine's execution flow.
        /// </summary>
        /// <returns>
        /// The initial state of type <see cref="State{GameStateMachine}"/> used to initiate the state machine.
        /// </returns>
        protected override State<GameStateMachine> GetInitialState()
        {
            var turnOrderState = new DefineTurnOrderState(this);
            
            AddState(new AssignRolesState(this));
            AddState(new CheckLastTurnState(this));
            AddState(new CheckRitualState(this));
            AddState(new CheckTriggerVoteState(this));
            AddState(turnOrderState);
            AddState(new DistributeCardsState(this));
            AddState(new EndGameState(this));
            AddState(new NightConsequencesState(this));
            AddState(new PlayerTurnState(this));
            
            return turnOrderState;
        }

        /// <summary>
        /// Determines whether the ritual is completed by checking if the required amounts of food and wood
        /// are collected, ensuring all collected items are valid.
        /// </summary>
        /// <returns>True if both the food and wood requirements are fulfilled; otherwise, false.</returns>
        private bool CheckRitualOver()
        {
            _ritualFoodCollected.RemoveAll(item => item == false);
            _ritualWoodCollected.RemoveAll(item => item == false);
            
            bool isFoodComplete = _ritualFoodCollected.Count == numberOfFoodToCompleteRitual && _ritualFoodCollected.All(item => item);
            bool isWoodComplete = _ritualWoodCollected.Count == numberOfWoodToCompleteRitual && _ritualWoodCollected.All(item => item);

            return isFoodComplete && isWoodComplete;

        }

        /// <summary>
        /// Adds a specified type of resource to the ritual process, tracking whether it is a real or fake resource.
        /// </summary>
        /// <param name="isHiddenList">If we add the resource to the real, or hidden list. Used for visibility when cycle is <see cref="Cycle.Night"/></param>
        /// <param name="resource">The type of resource to be added to the ritual, either Food or Wood.</param>
        /// <param name="isRealResource">Indicates whether the resource being added is real (true) or fake (false).</param>
        public void AddRessourceToRitual(bool isHiddenList, ResourceType resource, bool isRealResource)
        {
            switch (isHiddenList)
            {
                case true:
                    if (resource == ResourceType.Food)
                        _hiddenRitualFoodCollected.Add(isRealResource);
                    else if (resource == ResourceType.Wood)
                        _hiddenRitualWoodCollected.Add(isRealResource);
                    break;
                case false:
                    if (resource == ResourceType.Food)
                        _ritualFoodCollected.Add(isRealResource);
                    else if (resource == ResourceType.Wood)
                        _ritualWoodCollected.Add(isRealResource);
                    break;
            }
        }

        /// <summary>
        /// Synchronizes the public food and wood collection lists with their respective hidden lists
        /// by clearing the hidden lists and copying the contents of the public lists to them.
        /// </summary>
        public void CopyPublicToHidden()
        {
            _hiddenRitualFoodCollected.Clear();
            _hiddenRitualFoodCollected.AddRange(_ritualFoodCollected);
            _hiddenRitualWoodCollected.Clear();
            _hiddenRitualWoodCollected.AddRange(_ritualWoodCollected);
        }

        /// <summary>
        /// Synchronizes the hidden food and wood collection lists with their respective public lists
        /// by clearing the public lists and copying the contents of the hidden lists to them.
        /// </summary>
        public void CopyHiddenToPublic()
        {
            _ritualFoodCollected.Clear();
            _ritualFoodCollected.AddRange(_hiddenRitualFoodCollected);
            _ritualWoodCollected.Clear();
            _ritualWoodCollected.AddRange(_hiddenRitualWoodCollected);
        }

        #endregion

        #region Called By States

        /// <summary>
        /// Switches the current cycle of the game between Day and Night.
        /// </summary>
        /// <remarks>
        /// If the current cycle is Day, it transitions to Night. Otherwise, it transitions back to Day.
        /// Additionally, when transitioning from Night to Day, the turn counter is incremented,
        /// and the maximum turn condition is checked.
        /// </remarks>
        public void SwitchCycle()
        {
            Cycle newCycle;

            switch (Cycle)
            {
                case Cycle.Day:
                    newCycle = Cycle.Night;
                    break;
                case Cycle.Night:
                    newCycle = Cycle.Day;
                    _cptTurn++;
                    ServerManager.Instance.UpdateTurn(_cptTurn);
                    break;
                default:
                    throw new System.Exception("Invalid cycle value.");
            }
            
            if (ShowDebugLogs) Debug.LogWarning($"******************** Change cycle from {Cycle} to {newCycle} ! ********************");
            Cycle = newCycle;
            ServerManager.Instance.UpdateCycle(newCycle);
        }

        /// <summary>
        /// Checks if the current turn has reached or exceeded the maximum allowed turns.
        /// </summary>
        /// <returns>Returns true if the current turn is greater than or equal to the maximum turn limit; otherwise, false.</returns>
        public bool CheckMaximumTurnReached()
        {
            if (ShowDebugLogs) Debug.Log($"Current turn : {_cptTurn} / {maximumTurn}");
            return _cptTurn >= maximumTurn;
        }

        /// <summary>
        /// Determines whether it is time to initiate a voting phase based on the turn counter.
        /// Resets the turn counter if a voting phase is triggered.
        /// </summary>
        /// <returns>True if a voting phase should be initiated; otherwise, false.</returns>
        public bool CheckVotingTurn()
        {
            if (ShowDebugLogs) Debug.Log($"Current turn for vote: {_cptTurnForVote} / {triggerVoteEveryXTurn}");
            if (_cptTurnForVote >= triggerVoteEveryXTurn)
            {
                _cptTurnForVote = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Increments the counter tracking the number of turns passed until a vote is triggered.
        /// </summary>
        public void IncreaseCptTurnForVote() => _cptTurnForVote++;
        
        public void ResetCptTurnForVote() => _cptTurnForVote = 1;

        #endregion

        #region Called By ServerManager

        /// <summary>
        /// Register a player ID to maintain a reference to all players in the State Machine.
        /// </summary>
        /// <param name="playerID">The unique ID of the player assigned by the network.</param>
        public void RegisterPlayerID(ulong playerID)
        {
            PlayersID.Add(playerID);
        }

        /// <summary>
        /// Evaluates the card played during a player's turn and performs the necessary actions
        /// based on the current game cycle (Day or Night).
        /// </summary>
        /// <param name="playedCardID">The ID of the card that has been played.</param>
        /// <param name="origin">The ID of the player who does the action.</param>
        /// <param name="target">The ID of the target player, if applicable.</param>
        public void CheckCardPlayed(int playedCardID, ulong origin, ulong target)
        {
            GetConcreteState<PlayerTurnState>().CheckCardPlayed(playedCardID, origin, target);
        }
        
        /// <summary>
        /// Draws a specified number of cards from a given deck for a specified player.
        /// </summary>
        /// <param name="playerID">The unique identifier of the player who will receive the cards.</param>
        /// <param name="deckID">The identifier of the deck from which cards will be drawn.</param>
        /// <param name="amount">The number of cards to draw from the deck.</param>
        public void DrawCards(ulong playerID, int deckID, int amount)
        {
            var deck = DataCollection.Instance.GetDeck(deckID);
            if (deck == null || deck.Count == 0) return;
            
            Dictionary<ulong, List<int>> playerCards = new();
            playerCards[playerID] = new List<int>();
            
            int cardsToDraw = Mathf.Min(amount, deck.Count);
            for (var i = 0; i < cardsToDraw; i++)
            {
                int randomIndex = Random.Range(0, deck.Count);
                int cardID = deck[randomIndex].ID;
                
                playerCards[playerID].Add(cardID);
                deck.RemoveAt(randomIndex);
            }

            Utils.DictionaryToArrays(playerCards, out ulong[] targets, out int[][] cardsID);
            ServerManager.Instance.SendCardsToPlayersRpc(targets, cardsID);
        }

        #endregion
    }
}