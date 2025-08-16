namespace Wendogo
{
    /// <summary>
    /// Represents a state in the game state machine that handles logic pertaining
    /// to checking the completion of the last turn of the game cycle.
    /// </summary>
    public class CheckLastTurnState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state that checks the last turn in the game.
        /// </summary>
        public CheckLastTurnState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckLastTurn();
        }

        /// <summary>
        /// Checks whether the game has reached the maximum number of turns.
        /// Transitions to the appropriate game state based on the result.
        /// </summary>
        /// <remarks>
        /// It evaluates the current turn status by invoking the state machine's
        /// <see cref="GameStateMachine.CheckMaximumTurnReached"/> method.
        /// If the maximum turn count is reached, the state is changed to <see cref="EndGameState"/>.
        /// Otherwise, the state transitions to <see cref="PlayerTurnState"/>.
        /// </remarks>
        private void CheckLastTurn()
        {
            if (StateMachine.CheckMaximumTurnReached())
                StateMachine.ChangeState<EndGameState>();
            else
                StateMachine.ChangeState<PlayerTurnState>();
        }

        /// <summary>
        /// Reorders the list of player IDs by moving the last player in the list to the front.
        /// This ensures that the turn order is updated correctly at the end of a game turn.
        /// </summary>
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