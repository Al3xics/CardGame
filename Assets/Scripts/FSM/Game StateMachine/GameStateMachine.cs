using System;
using System.Collections.Generic;
using TMPro;
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
        public DataCollection DataCollectionScript;
        public int startingActionDeckAmount = 3;
        public int startingResourceDeckAmount = 2;
        public int pointPerTurn = 2;
        public Dictionary<ulong, List<PlayerAction>> NightActions = new();
        
        private int _cptTurn = 1;
        
        public List<ulong> PlayersID { get; private set; } = new();
        public Cycle Cycle { get; private set; } = Cycle.Day;
        public bool IsMaximumTurnReached {get; private set;} = false;
        public bool IsRitualOver {get; private set;} = false;
        public Dictionary<ulong, int> PlayersHealth { get; private set; } = new();

        #endregion

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        protected override State<GameStateMachine> GetInitialState()
        {
            var turnOrderState = new DefineTurnOrderState(this);
            
            AddState(new AllocateActionPointsState(this));
            AddState(new AssignRolesState(this));
            AddState(new CheckHealthState(this));
            AddState(new CheckLastTurnState(this));
            AddState(new CheckRitualState(this));
            AddState(turnOrderState);
            AddState(new DistributeCardsState(this));
            AddState(new DrawCardForPlayerState(this));
            AddState(new EndGameState(this));
            AddState(new NightConsequencesState(this));
            AddState(new PlayerTurnState(this));
            
            return turnOrderState;
        }

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
            GetConcreteState<DrawCardForPlayerState>().DrawCards(playerID, deckID, amount);
        }

        #endregion


        #region TO DELETE

        /* ----------------------------------------------- */
        /* ------------------ TO DELETE ------------------ */
        /* ----------------------------------------------- */
        
        public event Action EventAction;
        public TMP_Text turnText;
        public TMP_Text cycleText;

        protected override void Start()
        {
            base.Start();
            
            SetTurnText();
            SetCycleText();
        }
        
        public void NextState()
        {
            EventAction?.Invoke();
        }

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
                SetTurnText();
                CheckMaximumTurnReached();
            }
            
            Debug.Log($"Change cycle from {Cycle} to {newCycle} !");
            Cycle = newCycle;
            SetCycleText();
        }

        private void CheckMaximumTurnReached()
        {
            if (_cptTurn >= maximumTurn)
                IsMaximumTurnReached = true;
        }
        
        private void SetTurnText() => turnText.text = "Turn: " + _cptTurn;
        private void SetCycleText() => cycleText.text = Cycle.ToString();
        /* ----------------------------------------------- */
        /* ------------------ TO DELETE ------------------ */
        /* ----------------------------------------------- */

        #endregion
    }
}