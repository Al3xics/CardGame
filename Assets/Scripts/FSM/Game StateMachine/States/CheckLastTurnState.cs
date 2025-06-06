namespace Wendogo
{
    public class CheckLastTurnState : State<GameStateMachine>
    {
        public CheckLastTurnState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            GameStateMachine.Instance.EventAction += NextState;
        }

        private void NextState()
        {
            GameStateMachine.Instance.EventAction -= NextState;
            
            // Should only be executed just after the night
            // because incrementing cptTurn when changing from night to day
            if (GameStateMachine.Instance.IsMaximumTurnReached)
                StateMachine.ChangeState<EndGameState>();
            else
                StateMachine.ChangeState<AllocateActionPointsState>();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}