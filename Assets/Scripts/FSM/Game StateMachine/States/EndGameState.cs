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

        /// <summary>
        /// Determines and processes the outcome of the game by analyzing the end-game condition
        /// and evaluating the roles and statuses of all players. This method consolidates information
        /// from both active and eliminated players and triggers specific in-game events or effects
        /// based on the reason for the game’s conclusion.
        /// </summary>
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
                
                switch (StateMachine.EndGameReason)
                {
                    case EndGameReason.LastTurnEnded:
                        fxType = WhenLastTurnEnded(player);
                        break;
                        
                    case EndGameReason.RitualEnded:
                        fxType = WhenRitualEnded(player);
                        break;

                    case EndGameReason.SurvivorsDead:
                        fxType = WhenSurvivorsDead(player);
                        break;

                    case EndGameReason.WendogoDead:
                        fxType = WhenWendogoDead(player);
                        break;

                    default:
                        fxType = FXEventType.None;
                        break;
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

        /// <summary>
        /// Determines the type of visual effects event to trigger when the game ends due to the last turn being played.
        /// The result depends on the role of the player.
        /// </summary>
        /// <param name="player">The player for whom the end-game event determination is being made.</param>
        /// <returns>
        /// An FXEventType that specifies whether the player wins, loses, or if no event is triggered.
        /// </returns>
        private FXEventType WhenLastTurnEnded(PlayerController player)
        {
            return player.Role.Value switch
            {
                RoleType.Survivor => FXEventType.OnPlayerLose,
                RoleType.Wendogo => FXEventType.OnWendogoWin,
                _ => FXEventType.None
            };
        }

        /// <summary>
        /// Determines the appropriate visual effect to trigger when the end-game ritual concludes,
        /// based on the role of the player.
        /// </summary>
        /// <param name="player">The player whose role will determine the resulting visual effect.</param>
        /// <returns>An <see cref="FXEventType"/> value representing the visual effect to trigger.</returns>
        private FXEventType WhenRitualEnded(PlayerController player)
        {
            return player.Role.Value switch
            {
                RoleType.Survivor => FXEventType.OnPlayerWin,
                RoleType.Wendogo => FXEventType.OnWendogoLose,
                _ => FXEventType.None
            };
        }

        /// <summary>
        /// Determines the FXEventType to trigger when all survivors are dead.
        /// This method calculates the outcome based on the role of the player, triggering
        /// a win or loss effect depending on their role within the game.
        /// </summary>
        /// <param name="player">The player whose role determines the resulting FXEventType.</param>
        /// <returns>
        /// Returns the corresponding FXEventType based on the player's role:
        /// OnPlayerLose for Survivor or OnWendogoWin for Wendogo.
        /// </returns>
        private FXEventType WhenSurvivorsDead(PlayerController player)
        {
            return player.Role.Value switch
            {
                RoleType.Survivor => FXEventType.OnPlayerLose,
                RoleType.Wendogo => FXEventType.OnWendogoWin,
                _ => FXEventType.None
            };
        }

        /// <summary>
        /// Handles the event for when the Wendogo character has been defeated in the game.
        /// Determines the appropriate visual effect type based on the player's role.
        /// </summary>
        /// <param name="player">The player involved in the scenario, whose role will determine the resulting event type.</param>
        /// <returns>The type of visual effect event corresponding to the player's role in the context of Wendogo's defeat.</returns>
        private FXEventType WhenWendogoDead(PlayerController player)
        {
            return player.Role.Value switch
            {
                RoleType.Survivor => FXEventType.OnPlayerWin,
                RoleType.Wendogo => FXEventType.OnWendogoLose,
                _ => FXEventType.None
            };
        }

        /// <summary>
        /// Handles the logic that is executed when all animations have finished in the end-game state.
        /// This method is triggered by the OnAnimationFinished event in the ServerManager.
        /// Once invoked, it unsubscribes from the event, resets the end-game animation counter, and switches
        /// the game to the main menu scene.
        /// </summary>
        private void OnAnimationsFinished()
        {
            ServerManager.Instance.OnAnimationFinished -= OnAnimationsFinished;
            ServerManager.Instance.ResetEndGameAnimationFinishedCpt();
            ServerManager.Instance.ReturnToMenu();
        }
    }
}