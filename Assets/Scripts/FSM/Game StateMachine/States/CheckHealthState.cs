namespace Wendogo
{
    public class CheckHealthState : State<GameStateMachine>
    {
        public CheckHealthState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnCheckedHealth += NextState;
            CheckHealth();
        }

        private void CheckHealth()
        {
            foreach (var player in StateMachine.PlayersHealth)
            {
                // If dead
                if (player.Value == 0)
                {
                    // Remove player from the game (pass to viewer mode)
                    // Send to server manager to tell the player he is dead and need to change to viewer mode
                }
            }

            ServerManager.Instance.ChangePlayersHealth(StateMachine.PlayersHealth);
        }

        private void NextState()
        {
            ServerManager.Instance.OnCheckedHealth -= NextState;
            StateMachine.ChangeState<NightConsequencesState>();
        }
    }
}