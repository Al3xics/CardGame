using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents a state in the GameStateMachine where the ritual status is checked.
    /// </summary>
    public class CheckRitualState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state in the game state machine to check the status of an ongoing ritual.
        /// </summary>
        public CheckRitualState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckRitual();
        }

        /// <summary>
        /// This method handles the logic for advancing the game's state machine
        /// by evaluating whether the ritual is over or the current <see cref="Cycle"/> of the game.
        /// </summary>
        private void CheckRitual()
        {
            if (StateMachine.IsRitualOver)
            {
                Log("The Ritual is over.");
                StateMachine.ChangeState<EndGameState>();
                return;
            }

            Log("The Ritual is not over.");
            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    if (StateMachine.PreviousState is NightConsequencesState)
                        StateMachine.ChangeState<CheckTriggerVoteState>();
                    else
                        StateMachine.ChangeState<PlayerTurnState>();
                    break;
                case Cycle.Night:
                    LogError("[CheckRitualState] The game is in Night mode. This should not happen.");
                    break;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            
            if (StateMachine.Cycle == Cycle.Day && StateMachine.CurrentPlayerId >= StateMachine.PlayersID.Count)
            {
                StateMachine.CurrentPlayerId = 0;
                
                if (StateMachine.PreviousState is not NightConsequencesState)
                {
                    StateMachine.NightActions.Clear();
                    StateMachine.SwitchCycle();
                }
            }
        }
    }
}