namespace Wendogo
{
    /// <summary>
    /// Represents a state in the game flow where the win/lose conditions are checked.
    /// This state is responsible for evaluating the game's current state to determine
    /// if the wendogo is dead, or all survivors are dead to trigger the win/lose animation on each player.
    /// </summary>
    public class CheckWinLoseState : State<GameStateMachine>
    {
        private int _totalPlayers;
        
        /// <summary>
        /// Represents a state in the game where the win and lose conditions are evaluated.
        /// </summary>
        public CheckWinLoseState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
                            
            if (AutoSessionBootstrapper.AutoConnect)
                _totalPlayers = AutoSessionBootstrapper.ExpectedPlayersCount;
            else
                _totalPlayers = SessionManager.Instance.ActiveSession.Players.Count;
            
            CheckWinLose();
        }

        private void CheckWinLose()
        {
            // If at least one player is dead
            if (ServerManager.Instance.DeadPlayersId.Count > 0)
            {
                int survivorsDeadCount = 0;
                
                foreach (var playerId in ServerManager.Instance.DeadPlayersId)
                {
                    var deadPlayer = PlayerController.GetDeadPlayer(playerId);

                    // If the recovered player is valid and indeed dead
                    if (deadPlayer != null && deadPlayer.isDead.Value)
                    {
                        // If Wendogo -> EndGameState + end foreach
                        if (deadPlayer.Role.Value == RoleType.Wendogo)
                        {
                            StateMachine.EndGameReason = EndGameReason.WendogoDead;
                            StateMachine.ChangeState<EndGameState>();
                            return;
                        }

                        // Dead Survivor Counter
                        if (deadPlayer.Role.Value == RoleType.Survivor)
                        {
                            survivorsDeadCount++;

                            // If every survivor is dead -> EndGameState + end foreach
                            if (survivorsDeadCount >= _totalPlayers - 1)
                            {
                                StateMachine.EndGameReason = EndGameReason.SurvivorsDead;
                                StateMachine.ChangeState<EndGameState>();
                                return;
                            }
                        }
                    }
                }

                RedirectToState();
            }
            else
                RedirectToState();
        }

        /// <summary>
        /// Determines the next state to transition to based on the current cycle in the game,
        /// changing the state machine to either the CheckRitualState during the Day cycle
        /// or the NightConsequencesState during the Night cycle.
        /// </summary>
        private void RedirectToState()
        {
            if (ServerManager.Instance.currentCycle.Value == Cycle.Day) 
                StateMachine.ChangeState<CheckRitualState>();
            else if (ServerManager.Instance.currentCycle.Value == Cycle.Night)
                StateMachine.ChangeState<NightConsequencesState>();
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            if (StateMachine.Cycle == Cycle.Night)
                StateMachine.SwitchCycle();

        }
    }
}