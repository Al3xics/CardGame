using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        /// <summary>
        /// Represents the maximum number of turns allowed in the game.
        /// If the number of completed turns reaches this value, the game will end.
        /// </summary>
        [SerializeField] private int maximumTurn = 10;

        /// <summary>
        /// Reference to the DataCollection instance used to manage and retrieve various game-related data,
        /// such as decks and card configurations.
        /// </summary>
        public DataCollection dataCollectionScript;

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
        /// A dictionary used to store actions performed by players during the "Night" cycle of the game.
        /// </summary>
        /// <remarks>
        /// The keys of the dictionary represent the unique identifiers (IDs) of players, while the values are lists of <see cref="PlayerAction"/> objects.
        /// Each <see cref="PlayerAction"/> in a list corresponds to an action performed by the associated player during the night phase.
        /// This data structure is cleared at the end of each "Night" cycle after resolving related consequences. It is populated dynamically
        /// during the night phase as players make their moves.
        /// </remarks>
        public readonly Dictionary<ulong, List<PlayerAction>> NightActions = new();
        
        /// <summary>
        /// Tracks the current turn count within the game cycle.
        /// </summary>
        private int _cptTurn = 1;

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
        /// Represents a list containing the unique identifiers (IDs) of all players currently
        /// participating in the game. Used to manage player-specific data and turn orders.
        /// </summary>
        public List<ulong> PlayersID { get; private set; } = new();

        /// <summary>
        /// Represents the current <see cref="Cycle"/> of the game, which can be either Day or Night.
        /// The state of the cycle determines the flow of the game's behavior and logic.
        /// </summary>
        public Cycle Cycle { get; private set; } = Cycle.Day;

        /// <summary>
        /// Indicates whether the ritual in the game has been completed or not.
        /// This property is used to control the flow of game states, transitioning
        /// to the end game state if the ritual is complete or proceeding
        /// with the normal game cycle.
        /// </summary>
        public bool IsRitualOver {get; private set;} = false;

        /// <summary>
        /// Represents a mapping of player unique identifiers (IDs) to their current health values.
        /// </summary>
        /// <remarks>
        /// This property maintains the health status of all players in the game.
        /// Each player's health is mapped using their unique identifier (ulong) as the key and their health value (int) as the value.
        /// It is used to track and manage health-related gameplay mechanics throughout the game's lifecycle.
        /// Changes to player health are checked and used in various game states, such as in <see cref="NightConsequencesState"/>.
        /// </remarks>
        public Dictionary<ulong, int> PlayersHealth { get; private set; } = new();

        #endregion

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

            if (Cycle == Cycle.Day)
            {
                newCycle = Cycle.Night;
            }
            else
            {
                newCycle = Cycle.Day;
                _cptTurn++;
                CheckMaximumTurnReached();
            }
            
            Debug.Log($"Change cycle from {Cycle} to {newCycle} !");
            Cycle = newCycle;
        }

        /// <summary>
        /// Checks if the current turn has reached or exceeded the maximum allowed turns.
        /// </summary>
        /// <returns>Returns true if the current turn is greater than or equal to the maximum turn limit; otherwise, false.</returns>
        public bool CheckMaximumTurnReached()
        {
            return _cptTurn >= maximumTurn;
        }

        /// <summary>
        /// Determines whether it is time to initiate a voting phase based on the turn counter.
        /// Resets the turn counter if a voting phase is triggered.
        /// </summary>
        /// <returns>True if a voting phase should be initiated; otherwise, false.</returns>
        public bool CheckVotingTurn()
        {
            if (_cptTurnForVote >= triggerVoteEveryXTurn)
            {
                _cptTurnForVote = 1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Increments the counter tracking the number of turns passed until a vote is triggered.
        /// </summary>
        public void IncreaseCptTurnForVote() => _cptTurnForVote++;

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
        /// <param name="target">The ID of the target player, if applicable.</param>
        public void CheckCardPlayed(int playedCardID, ulong target)
        {
            GetConcreteState<PlayerTurnState>().CheckCardPlayed(playedCardID, target);
        }
        
        /// <summary>
        /// Draws a specified number of cards from a given deck for a specified player.
        /// </summary>
        /// <param name="playerID">The unique identifier of the player who will receive the cards.</param>
        /// <param name="deckID">The identifier of the deck from which cards will be drawn.</param>
        /// <param name="amount">The number of cards to draw from the deck.</param>
        public void DrawCards(ulong playerID, int deckID, int amount)
        {
            var deck = dataCollectionScript.GetDeck(deckID);
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
            ServerManager.Instance.SendCardsToPlayersServerRpc(targets, cardsID);
        }

        #endregion
    }
}