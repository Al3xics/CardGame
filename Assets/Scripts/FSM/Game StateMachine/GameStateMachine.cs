using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

        /* --------------- Show in Inspector : Debug --------------- */
        /// <summary>
        /// Allows setting a custom turn order for players.
        /// If left empty, the order will be randomized.
        /// </summary>
        [Tooltip("Provide a custom turn order for players. Leave empty for random order.")]
        public List<ulong> customTurnOrder;

        /// <summary>
        /// Specifies a custom ID for the Wendogo character.
        /// If set to -1, a random Wendogo ID will be assigned.
        /// </summary>
        [Tooltip("Provide a custom ID for the Wendogo. Leave '-1' for default .")]
        public int wendogoId = -1;
        
        /* --------------- Show in Inspector : Game Settings --------------- */
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

        /// <summary>
        /// The card effect that is executed when the vote is triggered every X turn.
        /// </summary>
        public GroupAttack groupVoteEffectEveryXTurn;

        /* --------------- Hide in Inspector --------------- */
        private int _cptTurn = 1;

        /// <summary>
        /// Tracks the current turn count within the game cycle.
        /// </summary>
        public int CptTurn
        {
            get => _cptTurn;
            protected set
            {
                if (_cptTurn == value) return;
                _cptTurn = value;
                OnTurnChanged?.Invoke(value);
            }
        }

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
        /// Represents a list containing the unique identifiers (IDs) of all players currently
        /// participating in the game. Used to manage player-specific data and turn orders.
        /// </summary>
        public List<ulong> PlayersID { get; set; } = new();

        /// <summary>
        /// Represents the queue that determines the turn order of players in the game.
        /// This property provides access to the sequential arrangement of player IDs,
        /// allowing players to take actions in the correct order.
        /// </summary>
        public Queue<ulong> TurnQueue { get; private set; } = new();

        /// <summary>
        /// Gets the ID of the current player whose turn is active.
        /// This property retrieves the player ID from the front of the turn queue.
        /// If the queue is empty, it returns 0 as a default value.
        /// </summary>
        public ulong CurrentPlayerId => TurnQueue.Count > 0 ? TurnQueue.Peek() : 0;

        /// <summary>
        /// Represents the number of players active at the start of a gameplay cycle.
        /// This value is used to track and manage the state of the turn sequence during a cycle
        /// and can be dynamically adjusted based on player status updates, such as disconnections or eliminations.
        /// </summary>
        public int playersAtCycleStart;

        /// <summary>
        /// Tracks the number of players who have taken their turns in the current cycle.
        /// This variable is incremented as each player's turn concludes
        /// and is used to determine the progression and state transitions of the game cycle.
        /// </summary>
        public int playersPlayedThisCycle;

        /// <summary>
        /// Stores the set of player IDs that have completed their turn in the current cycle.
        /// This ensures that each player is tracked as having played during a single gameplay cycle.
        /// </summary>
        private readonly HashSet<ulong> _playedThisCycle = new();

        private Cycle _cycle = Cycle.Day;

        /// <summary>
        /// Represents the current <see cref="Cycle"/> of the game, which can be either Day or Night.
        /// The state of the cycle determines the flow of the game's behavior and logic.
        /// </summary>
        public Cycle Cycle
        {
            get => _cycle;
            protected set
            {
                if (_cycle == value) return;
                _cycle = value;
                OnCycleChanged?.Invoke(value);
            }
        }

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

        /// <summary>
        /// Represent the state of the food. If it is false, then this resource
        /// is blocked and inaccessible for players.
        /// </summary>
        private bool _canScavengeFood = true;

        /// <summary>
        /// Represent the state of the wood. If it is false, then this resource
        /// is blocked and inaccessible for players.
        /// </summary>
        private bool _canScavengeWood = true;

        #endregion

        #region Action

        public event Action<int> OnTurnChanged;
        public event Action<Cycle> OnCycleChanged;

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

            if (groupVoteEffectEveryXTurn == null)
                throw new Exception($"The variable 'Group Vote Effect Every X Turn' inside the 'GameStateMachine' script is null.");

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
        /// <param name="value">The number of resources to add in the list.</param>
        public void AddRessourceToRitual(bool isHiddenList, ResourceType resource, bool isRealResource, int value)
        {
            List<bool> targetList = null;

            if (isHiddenList)
            {
                targetList = resource switch
                {
                    ResourceType.Food => _hiddenRitualFoodCollected,
                    ResourceType.Wood => _hiddenRitualWoodCollected,
                    _ => null
                };
            }
            else
            {
                targetList = resource switch
                {
                    ResourceType.Food => _ritualFoodCollected,
                    ResourceType.Wood => _ritualWoodCollected,
                    _ => null
                };

                var ritualValue = resource == ResourceType.Food
                    ? ServerManager.Instance._foodInRitual
                    : ServerManager.Instance._woodInRitual;

                ritualValue.Value += value;
            }

            if (targetList == null) return;

            // Add resources
            targetList.AddRange(Enumerable.Repeat(isRealResource, value));

            // Clamp to the maximum size
            int max = resource == ResourceType.Food ? numberOfFoodToCompleteRitual : numberOfWoodToCompleteRitual;
            if (targetList.Count > max)
                targetList.RemoveRange(max, targetList.Count - max);
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

            ServerManager.Instance._foodInRitual.Value = _ritualFoodCollected.Count();
            ServerManager.Instance._woodInRitual.Value = _ritualWoodCollected.Count();
        }

        /// <summary>
        /// Retrieves a list of night actions that have a priority greater than zero.
        /// </summary>
        /// <returns>
        /// A list of <see cref="PlayerAction"/> objects from the NightActionsForMorning collection
        /// where the actions have a positive CardPriorityIndex value.
        /// </returns>
        public List<PlayerAction> GetNightActionsWithPriority()
        {
            return NightActions.Where(c => c.CardPriorityIndex > 0).ToList();
        }

        #endregion

        #region Called By States

        /// <summary>
        /// Configures the order of players' turns by initializing the turn queue.
        /// If a custom turn order is specified, it will use that order; otherwise,
        /// it shuffles the player IDs to create a random turn order.
        /// </summary>
        public void InitializeTurnQueue()
        {
            if (customTurnOrder is { Count: > 0 })
                TurnQueue = new Queue<ulong>(customTurnOrder);
            else
            {
                var shuffled = new List<ulong>(PlayersID);
                Shuffle(shuffled);
                TurnQueue = new Queue<ulong>(shuffled);
            }

            ResetPlayersCycle();
        }

        /// <summary>
        /// Resets the players' cycle data by reinitializing the count of players at the beginning of the cycle
        /// and setting the number of players who have played in the current cycle to zero.
        /// </summary>
        private void ResetPlayersCycle()
        {
            playersAtCycleStart = TurnQueue.Count; // number of living at the start of the cycle
            playersPlayedThisCycle = 0;
            _playedThisCycle.Clear();
        }
        
        /// <summary>
        /// Randomly shuffles the elements of the given list in place.
        /// The order of elements in the list is randomized.
        /// </summary>
        /// <param name="list">The list of type <c>ulong</c> representing the player's unique IDs to be shuffled.</param>
        private void Shuffle(List<ulong> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randIndex = Random.Range(i, list.Count);
                (list[i], list[randIndex]) = (list[randIndex], list[i]);
            }
        }

        /// <summary>
        /// Ends the current player's turn by removing them from the front of the turn queue.
        /// If the player is still an active participant, they are re-added to the back of the queue.
        /// </summary>
        public void EndCurrentPlayerTurn()
        {
            if (TurnQueue.Count == 0) return;

            ulong finishedPlayer = TurnQueue.Dequeue();

            // This player played well in THIS cycle
            _playedThisCycle.Add(finishedPlayer);

            // If he is still alive, we put him back at the end of the queue
            if (PlayersID.Contains(finishedPlayer))
                TurnQueue.Enqueue(finishedPlayer);
        }

        /// <summary>
        /// Reorders the queue of player IDs by moving the last player in the queue to the front.
        /// This ensures that the turn order is updated correctly at the end of a game turn.
        /// </summary>
        /// <remarks>For example, if the queue is <c>[0,1,2,3]</c> and <c>shift = 1</c>, the result will be <c>[3,0,1,2]</c>.</remarks>
        public void ReorderPlayersTurn(int shift = 1)
        {
            if (TurnQueue.Count == 0 || shift <= 0) return;

            var queueList = TurnQueue.ToList();

            for (int i = 0; i < shift; i++)
            {
                // Remove the last element and add it at the beginning of the list
                var last = queueList[^1];
                queueList.RemoveAt(queueList.Count - 1);
                queueList.Insert(0, last);
            }

            TurnQueue = new Queue<ulong>(queueList);
        }

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
                    CptTurn++;
                    break;
                default:
                    throw new System.Exception("Invalid cycle value.");
            }

            if (ShowDebugLogs) Debug.LogWarning($"******************** Change cycle from {Cycle} to {newCycle} ! ********************");
            ServerManager.Instance.AskToUnlockResourcesRpc(false, false);
            ServerManager.Instance.AskToUnlockResourcesRpc(true, false);
            ServerManager.Instance.nightActions.Clear();
            Cycle = newCycle;
            
            ResetPlayersCycle();
        }

        /// <summary>
        /// Checks if the current turn has reached or exceeded the maximum allowed turns.
        /// </summary>
        /// <returns>Returns true if the current turn is greater than or equal to the maximum turn limit; otherwise, false.</returns>
        public bool CheckMaximumTurnReached()
        {
            if (ShowDebugLogs) Debug.Log($"Current turn : {CptTurn} / {maximumTurn}");
            return CptTurn > maximumTurn;
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
        /// Synchronize the value from the server and the state machine.
        /// </summary>
        public void ForceInitialSync()
        {
            OnTurnChanged?.Invoke(CptTurn);
            OnCycleChanged?.Invoke(Cycle);
        }

        /// <summary>
        /// Register a player ID to maintain a reference to all players in the State Machine.
        /// </summary>
        /// <param name="playerID">The unique ID of the player assigned by the network.</param>
        public void RegisterPlayerID(ulong playerID)
        {
            PlayersID.Add(playerID);
        }

        /// <summary>
        /// The player is dead.
        /// Unregister a player ID to maintain a reference to all players in the State Machine.
        /// </summary>
        /// <param name="playerID">The unique ID of the player you want to remove.</param>
        public void UnregisterPlayerID(ulong playerID)
        {
            if (!PlayersID.Contains(playerID))
                return;

            PlayersID.Remove(playerID);

            // Was he in the queue at the time of death?
            bool wasInQueue = TurnQueue.Contains(playerID);
            // Had he ALREADY played in the current cycle?
            bool alreadyPlayedThisCycle = _playedThisCycle.Contains(playerID);

            // Complete tail removal
            TurnQueue = new Queue<ulong>(TurnQueue.Where(p => p != playerID));

            // Only decrements the cycle quota if the player has not yet played
            if (wasInQueue && !alreadyPlayedThisCycle && playersPlayedThisCycle < playersAtCycleStart)
                playersAtCycleStart = Math.Max(0, playersAtCycleStart - 1);
        }

        /// <summary>
        /// Evaluates the card played during a player's turn and performs the necessary actions
        /// based on the current game cycle (Day or Night).
        /// </summary>
        /// <param name="playedCardID">The ID of the card that has been played.</param>
        /// <param name="origin">The ID of the player who does the action.</param>
        /// <param name="target">The ID of the target player, if applicable.</param>
        /// <param name="nbFood">Specific to the BuildRitual card. If different from -1, then the BuildRitual card was played.</param>
        /// <param name="nbWood">Specific to the BuildRitual card. If different from -1, then the BuildRitual card was played.</param>
        public void CheckCardPlayed(int playedCardID, ulong origin, ulong target, int nbFood, int nbWood)
        {
            GetConcreteState<PlayerTurnState>().CheckCardPlayed(playedCardID, origin, target, nbFood, nbWood);
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

        public void AskToUnlockResources(bool isFood, bool isBlock)
        {
            if (isFood)
                _canScavengeFood = !isBlock;
            else
                _canScavengeWood = !isBlock;
        }

        public bool GetCanScavengeFood() => _canScavengeFood;

        public bool GetCanScavengeWood() => _canScavengeWood;

        #endregion
    }
}