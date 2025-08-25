namespace Wendogo
{
    /// <summary>
    /// Represents the state where the turn order is defined for the game.
    /// This state performs the logic required to shuffle the player order
    /// and transitions to the <see cref="AssignRolesState"/>.
    /// </summary>
    public class DefineTurnOrderState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state within the game that defines the turn order for players.
        /// </summary>
        public DefineTurnOrderState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();

            if (AutoSessionBootstrapper.AutoConnect &&
                StateMachine.PlayersID.Count < AutoSessionBootstrapper.ExpectedPlayersCount)
            {
                throw new System.Exception("Not enough players to start the game.");
            }

            StateMachine.InitializeTurnQueue();
            StateMachine.ChangeState<AssignRolesState>();
        }
    }
}