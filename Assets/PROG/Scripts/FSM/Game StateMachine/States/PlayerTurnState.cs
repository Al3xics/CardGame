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
        /// Represents the ID of the current player whose turn is active in the game.
        /// This variable helps manage the game flow by tracking which player's turn is currently in progress.
        /// It is incremented sequentially to move to the next player in the turn order.
        /// </summary>
        private int _currentPlayerId = 0;
        
        /// <summary>
        /// Represents the state of the game during a player's turn.
        /// </summary>
        public PlayerTurnState(GameStateMachine stateMachine) : base(stateMachine) { }
        
        public override void OnEnter()
        {
            base.OnEnter();
            StartPlayerTurn(_currentPlayerId);
        }

        /// <summary>
        /// Initiates the turn for the specified player, setting up the necessary state for the turn.
        /// </summary>
        /// <param name="id">The ID of the player whose turn is starting.</param>
        private void StartPlayerTurn(int id)
        {
            Debug.Log("Next Player Turn");
            ServerManager.Instance.OnPlayerTurnEnded += NextPlayer;
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
                    ulong currentPlayer = StateMachine.PlayersID[_currentPlayerId];

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
        private void NextPlayer()
        {
            ServerManager.Instance.OnPlayerTurnEnded -= NextPlayer;
            
            _currentPlayerId++;
            
            if(_currentPlayerId == StateMachine.PlayersID.Count)
            {
                NextState();
                return;
            }
        
            StartPlayerTurn(_currentPlayerId);
        }

        /// <summary>
        /// Transitions the game state from the current <see cref="PlayerTurnState"/> to the <see cref="CheckRitualState"/>.
        /// </summary>
        private void NextState()
        {
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}