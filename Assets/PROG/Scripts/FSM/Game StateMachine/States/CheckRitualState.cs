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