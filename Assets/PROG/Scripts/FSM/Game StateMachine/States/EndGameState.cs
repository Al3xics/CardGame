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
    }
}