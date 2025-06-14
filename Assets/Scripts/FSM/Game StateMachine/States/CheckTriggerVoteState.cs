namespace Wendogo
{
    public class CheckTriggerVoteState : State<GameStateMachine>
    {
        public CheckTriggerVoteState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckTriggerVote();
        }

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