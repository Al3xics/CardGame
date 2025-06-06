namespace Wendogo
{
    public class CheckRitualState : State<GameStateMachine>
    {
        public CheckRitualState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckRitual();
        }

        private void CheckRitual()
        {
            if (GameStateMachine.Instance.IsRitualOver)
                StateMachine.ChangeState<EndGameState>();
            else
                switch (GameStateMachine.Instance.Cycle)
                {
                    case Cycle.Day:
                        StateMachine.ChangeState<CheckLastTurnState>();
                        break;
                    case Cycle.Night:
                        StateMachine.ChangeState<CheckHealthState>();
                        break;
                }
        }

        public override void OnExit()
        {
            base.OnExit();
            GameStateMachine.Instance.SwitchCycle();
        }
    }
}