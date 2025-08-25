using System.Collections.Generic;

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
            // Merge both players' lists: alive (PlayersById) and dead (DeadPlayersId)
            var allPlayers = new Dictionary<ulong, PlayerController>();
            
            foreach (var (playerId, player) in ServerManager.Instance.PlayersById)
            {
                allPlayers[playerId] = player;
            }
            
            foreach (var playerId in ServerManager.Instance.DeadPlayersId)
            {
                var deadPlayer = PlayerController.GetDeadPlayer(playerId);
                if (deadPlayer != null) allPlayers[playerId] = deadPlayer;
            }

            // Iterate through all players and broadcast the FX event based on their role
            foreach (var (playerId, player) in allPlayers)
            {
                FXEventType fxType;

                if (StateMachine.IsRitualOver)
                {
                    fxType = player.Role.Value switch
                    {
                        RoleType.Survivor => FXEventType.OnPlayerWin,
                        RoleType.Wendogo => FXEventType.OnWendogoLose,
                        _ => FXEventType.None
                    };
                }
                else
                {
                    fxType = player.Role.Value switch
                    {
                        RoleType.Survivor => FXEventType.OnPlayerLose,
                        RoleType.Wendogo => FXEventType.OnWendogoWin,
                        _ => FXEventType.None
                    };
                }

                if (fxType != FXEventType.None)
                {
                    ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
                    {
                        fxType = fxType,
                        playerID = playerId
                    });
                }
            }
        }

        private void OnAnimationsFinished()
        {
            ServerManager.Instance.OnAnimationFinished -= OnAnimationsFinished;
            ServerManager.Instance.ResetEndGameAnimationFinishedCpt();
            ServerManager.Instance.ReturnToMenu();
        }
    }
}