namespace Wendogo
{
    public class CheckLastTurnState : State<GameStateMachine>
    {
        public CheckLastTurnState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckLastTurn();
        }

        private void CheckLastTurn()
        {
            // Should only be executed just after the night
            // because incrementing cptTurn when changing from night to day
            if (StateMachine.CheckMaximumTurnReached())
                StateMachine.ChangeState<EndGameState>();
            else
                StateMachine.ChangeState<PlayerTurnState>();
        }

        private void ReorderPlayersTurn()
        {
            if (StateMachine.PlayersID == null || StateMachine.PlayersID.Count <= 1) return;

            // Take the last element, remove it, then add it to the front of the list
            ulong last = StateMachine.PlayersID[^1];
            StateMachine.PlayersID.RemoveAt(StateMachine.PlayersID.Count - 1);
            StateMachine.PlayersID.Insert(0, last);
        }


        public override void OnExit()
        {
            base.OnExit();
            ReorderPlayersTurn();
        }
    }
}