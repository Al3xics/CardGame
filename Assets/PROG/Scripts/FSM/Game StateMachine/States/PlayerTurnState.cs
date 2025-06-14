using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    public class PlayerTurnState : State<GameStateMachine>
    {
        private int _currentPlayerId = 0;
        
        public PlayerTurnState(GameStateMachine stateMachine) : base(stateMachine) { }
        
        public override void OnEnter()
        {
            base.OnEnter();
            StartPlayerTurn(_currentPlayerId);
        }

        private void StartPlayerTurn(int id)
        {
            Debug.Log("Next Player Turn");
            ServerManager.Instance.OnPlayerTurnEnded += NextPlayer;
            ServerManager.Instance.PlayerTurnServerServerRpc(StateMachine.PlayersID[id]);
        }

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

        private void NextState()
        {
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}