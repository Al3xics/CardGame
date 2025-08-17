using System;
using Unity.Services.Analytics;

namespace Wendogo
{
    /// <summary>
    /// Represents the state where a player's turn is handled within the game state machine.
    /// </summary>
    public class PlayerTurnState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state of the game during a player's turn.
        /// </summary>
        public PlayerTurnState(GameStateMachine stateMachine) : base(stateMachine) { }
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            if (StateMachine.Cycle == Cycle.Night)
            {
                ServerManager.Instance.SynchronizePlayerValuesRpc(true);
                StateMachine.CopyPublicToHidden();
            }
            
            StartPlayerTurn();
        }

        /// <summary>
        /// Initiates the turn for the specified player, setting up the necessary state for the turn.
        /// </summary>
        private void StartPlayerTurn()
        {
            var playerId = StateMachine.CurrentPlayerId;
            Log($"Player {playerId} Begin Turn");
            ServerManager.Instance.OnPlayerTurnEnded += OnPlayerTurnEnded;
            ServerManager.Instance.SetPlayerTurnStateRpc(true, playerId);
            ServerManager.Instance.PlayerTurnRpc(playerId);
            ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
            {
                fxType = FXEventType.OnPlayerTurn,
                playerID = playerId
            });
        }

        /// <summary>
        /// Checks the card played by a player during the game and resolves its effects based on the current game cycle.
        /// </summary>
        /// <param name="playedCardID">The unique identifier of the card that was played.</param>
        /// <param name="origin">The unique identifier of the player who played the card.</param>
        /// <param name="target">The unique identifier of the target player, if applicable.</param>
        /// <param name="nbFood">Specific to the BuildRitual card. If different from -1, then the BuildRitual card was played.</param>
        /// <param name="nbWood">Specific to the BuildRitual card. If different from -1, then the BuildRitual card was played.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the game cycle value is invalid or unhandled.</exception>
        public void CheckCardPlayed(int playedCardID, ulong origin, ulong target, int nbFood, int nbWood)
        {
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardID);

            // if true, it is the BuildRitual card (there should not be any other card that uses this value)
            if (nbFood > -1 || nbWood > -1 && card.CardEffect is BuildRitual)
                ServerManager.Instance.ApplyBuildRitualRpc(playedCardID, origin, nbFood, nbWood);
            
            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    // For passive cards
                    if (card.isPassive)
                    {
                        AnalyticsManager.Instance.RecordEvent(new CustomEvent("passiveCardPlayed"));
                        ServerManager.Instance.FinishedPassiveCardPlayedRpc(playedCardID, origin, false);
                        return;
                    }
                    
                    // For active cards
                    ServerManager.Instance.TryApplyPassiveRpc(playedCardID, origin, target);
                    break;
                
                case Cycle.Night:
                    // For passive cards
                    if (card.isPassive)
                    {
                        AnalyticsManager.Instance.RecordEvent(new CustomEvent("passiveCardPlayed"));
                        ServerManager.Instance.FinishedPassiveCardPlayedRpc(playedCardID, origin, true);
                        return;
                    }


                    PlayerAction action;
                    StateMachine.NightActions.Add(action = new PlayerAction()
                    {
                        CardId = playedCardID,
                        CardPriorityIndex = card.nightPriorityIndex,
                        OriginId = origin,
                        TargetId = target
                    });

                    ServerManager.Instance.nightActions.Add(action);

                    if (card.nightPriorityIndex <= 0) // Handle cards without priority
                        ServerManager.Instance.TryApplyPassiveRpc(playedCardID, origin, target);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(StateMachine.Cycle), StateMachine.Cycle, "The cycle is not valid.");
            }
        }

        /// <summary>
        /// Advances the game to the next player's turn.
        /// </summary>
        private void OnPlayerTurnEnded()
        {
            Log($"Player {StateMachine.CurrentPlayerIndex} End Turn");
            ServerManager.Instance.OnPlayerTurnEnded -= OnPlayerTurnEnded;
            ServerManager.Instance.SetPlayerTurnStateRpc(false, StateMachine.CurrentPlayerId);

            StateMachine.CurrentPlayerIndex++;
            bool isLastPlayer = StateMachine.CurrentPlayerIndex >= StateMachine.PlayersID.Count;

            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    StateMachine.ChangeState<CheckRitualState>();
                    break;

                case Cycle.Night:
                    if (isLastPlayer)
                        StateMachine.ChangeState<NightConsequencesState>();
                    else
                        StartPlayerTurn();
                    
                    break;
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            if (StateMachine.Cycle == Cycle.Night)
                StateMachine.SwitchCycle();

        }
    }
}