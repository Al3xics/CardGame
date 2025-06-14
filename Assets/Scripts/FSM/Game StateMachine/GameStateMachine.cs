using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wendogo
{
    public class GameStateMachine : StateMachine<GameStateMachine>
    {
        #region Instance
        
        public static GameStateMachine Instance { get; private set; }

        #endregion
        
        #region Variables
        
        [SerializeField] private int maximumTurn = 10;
        public DataCollection dataCollectionScript;
        public int startingActionDeckAmount = 3;
        public int startingResourceDeckAmount = 2;
        public int triggerVoteEveryXTurn = 2;
        public int pointPerTurn = 2;
        public readonly Dictionary<ulong, List<PlayerAction>> NightActions = new();
        
        private int _cptTurn = 1;
        private int _cptTurnForVote = 1;
        
        public List<ulong> PlayersID { get; private set; } = new();
        public Cycle Cycle { get; private set; } = Cycle.Day;
        public bool IsRitualOver {get; private set;} = false;
        public Dictionary<ulong, int> PlayersHealth { get; private set; } = new();

        #endregion

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            
            if (!AutoSessionBootstrapper.AutoConnect)
                ServerManager.Instance.InitializePlayers();
        }

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

        public bool CheckMaximumTurnReached()
        {
            return _cptTurn >= maximumTurn;
        }

        public bool CheckVotingTurn()
        {
            if (_cptTurnForVote >= triggerVoteEveryXTurn)
            {
                _cptTurnForVote = 1;
                return true;
            }

            return false;
        }

        public void IncreaseCptTurnForVote() => _cptTurnForVote++;

        #endregion

        #region Called By ServerManager

        /// <summary>
        /// Register a player ID to have a reference to all players in the State Machine.
        /// </summary>
        /// <param name="playerID">The ID of the player spawned by the network.</param>
        public void RegisterPlayerID(ulong playerID)
        {
            PlayersID.Add(playerID);
        }

        public void CheckCardPlayed(int playedCardID, ulong target)
        {
            GetConcreteState<PlayerTurnState>().CheckCardPlayed(playedCardID, target);
        }
        
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