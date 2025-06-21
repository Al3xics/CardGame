using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents the state where a player's turn is handled within the game state machine.
    /// </summary>
    public class PlayerTurnState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state of the game during a player's turn.
        /// </summary>
        public PlayerTurnState(GameStateMachine stateMachine) : base(stateMachine) { }
        
        public override void OnEnter()
        {
            base.OnEnter();
            StartPlayerTurn(StateMachine.CurrentPlayerId);
        }

        /// <summary>
        /// Initiates the turn for the specified player, setting up the necessary state for the turn.
        /// </summary>
        /// <param name="id">The ID of the player whose turn is starting.</param>
        private void StartPlayerTurn(int id)
        {
            Log($"Player {StateMachine.CurrentPlayerId} Begin Turn");
            ServerManager.Instance.OnPlayerTurnEnded += OnPlayerTurnEnded;
            ServerManager.Instance.PlayerTurnServerServerRpc(StateMachine.PlayersID[id]);
        }

        // todo
        public void CheckCardPlayed(int playedCardID, ulong target)
        {
            /*
             * Si un player per de la vie, on ajoute un booléan à un dictionnaire avec la key l'id du player, et la value sa vie
             * 
             */
            
            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    
                    ServerManager.Instance.SendDataServerServerRpc();
                    break;
                
                case Cycle.Night:
                    ulong currentPlayer = StateMachine.PlayersID[StateMachine.CurrentPlayerId];

                    if (!StateMachine.NightActions.ContainsKey(currentPlayer))
                        StateMachine.NightActions[currentPlayer] = new List<PlayerAction>();

                    StateMachine.NightActions[currentPlayer].Add(new PlayerAction
                    {
                        CardId = playedCardID,
                        TargetId = target
                    });

                    ServerManager.Instance.FinishedCheckCardPlayed();
                    break;
            }
        }

        /// <summary>
        /// Advances the game to the next player's turn.
        /// </summary>
        private void OnPlayerTurnEnded()
        {
            Log($"Player {StateMachine.CurrentPlayerId} End Turn");
            ServerManager.Instance.OnPlayerTurnEnded -= OnPlayerTurnEnded;

            StateMachine.CurrentPlayerId++;
            bool isLastPlayer = StateMachine.CurrentPlayerId >= StateMachine.PlayersID.Count;

            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    StateMachine.ChangeState<CheckRitualState>();
                    break;

                case Cycle.Night:
                    if (isLastPlayer)
                        StateMachine.ChangeState<NightConsequencesState>();
                    else
                        StartPlayerTurn(StateMachine.CurrentPlayerId);
                    
                    break;
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            if (StateMachine.Cycle == Cycle.Night)
                StateMachine.SwitchCycle();
        }
    }
}