namespace Wendogo
{
    /// <summary>
    /// Represents the final state in the game where all gameplay processes have concluded.
    /// </summary>
    public class EndGameState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the end-game state within the game's state machine.
        /// This state signifies the completion of a game session and is used to
        /// handle any logic required at the conclusion of the game lifecycle.
        /// </summary>
        public EndGameState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnAnimationFinished += OnAnimationsFinished;
            CheckWhoWon();
        }

        private void CheckWhoWon()
        {
            // if true, survivors WIN, else wendogo WIN
            ServerManager.Instance.StartPlayAnimationRpc(new AnimationParams
            {
                animatorName = AnimatorName.WinLoseUI,
                waitForAnimation = true,
                isSurvivorWin = StateMachine.IsRitualOver,
            });
        }

        private void OnAnimationsFinished()
        {
            ServerManager.Instance.OnAnimationFinished -= OnAnimationsFinished;
            ServerManager.Instance.ResetEndGameAnimationFinishedCpt();
            ServerManager.Instance.ReturnToMenu();
        }
    }
}