namespace Wendogo
{
    /// <summary>
    /// Represents a state in the GameStateMachine where the trigger for initiating a vote is checked.
    /// </summary>
    public class CheckTriggerVoteState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents a state within the game logic that checks whether certain conditions for triggering a vote have been met.
        /// </summary>
        public CheckTriggerVoteState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckTriggerVote();
        }

        // todo
        private void CheckTriggerVote()
        {
            if (StateMachine.CheckVotingTurn())
            {
                // We are in a voting state, so do things
                // todo
            }
            
            StateMachine.ChangeState<CheckLastTurnState>();
        }

        public override void OnExit()
        {
            base.OnExit();
            StateMachine.IncreaseCptTurnForVote();
        }
    }
}