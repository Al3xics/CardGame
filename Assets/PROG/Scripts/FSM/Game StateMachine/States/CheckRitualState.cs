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
        /// by evaluating whether the ritual is over or the current cycle of the game.
        /// If the ritual is completed, the state transitions to <see cref="EndGameState"/>.
        /// Otherwise, it checks the cycle and transitions to either <see cref="CheckTriggerVoteState"/>
        /// for the Day cycle or <see cref="NightConsequencesState"/> for the Night cycle.
        /// </summary>
        private void CheckRitual()
        {
            if (StateMachine.IsRitualOver)
                StateMachine.ChangeState<EndGameState>();
            else
                switch (StateMachine.Cycle)
                {
                    case Cycle.Day:
                        StateMachine.ChangeState<CheckTriggerVoteState>();
                        break;
                    case Cycle.Night:
                        StateMachine.ChangeState<NightConsequencesState>();
                        break;
                }
        }

        public override void OnExit()
        {
            base.OnExit();
            StateMachine.SwitchCycle();
        }
    }
}