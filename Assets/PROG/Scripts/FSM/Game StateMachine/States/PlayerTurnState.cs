using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
                ServerManager.Instance.SynchronizePlayerValuesServerRpc(true);
            
            StartPlayerTurn(StateMachine.CurrentPlayerId);
        }

        /// <summary>
        /// Initiates the turn for the specified player, setting up the necessary state for the turn.
        /// </summary>
        /// <param name="id">The ID of the player whose turn is starting.</param>
        private void StartPlayerTurn(int id)
        {
            Log($"Player {StateMachine.CurrentPlayerId} Begin Turn");
            ServerManager.Instance.OnPlayerTurnEnded += OnPlayerTurnEnded;
            ServerManager.Instance.PlayerTurnServerRpc(StateMachine.PlayersID[id]);
        }

        /// <summary>
        /// Checks the card played by a player during the game and resolves its effects based on the current game cycle.
        /// </summary>
        /// <param name="playedCardID">The unique identifier of the card that was played.</param>
        /// <param name="origin">The unique identifier of the player who played the card.</param>
        /// <param name="target">The unique identifier of the target player, if applicable.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the game cycle value is invalid or unhandled.</exception>
        public void CheckCardPlayed(int playedCardID, ulong origin, ulong target)
        {
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardID);
            
            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    // For passive cards
                    if (card.isPassive)
                    {
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
                        ServerManager.Instance.FinishedPassiveCardPlayedRpc(playedCardID, origin, true);
                        return;
                    }
                    
                    if (card.nightPriorityIndex != 0) // Add active card to NightActions if it has a priority index
                    {
                        StateMachine.NightActions.Add(new PlayerAction
                        {
                            CardId = playedCardID,
                            CardPriorityIndex = card.nightPriorityIndex,
                            OriginId = origin,
                            TargetId = target
                        });
                    }
                    else // Handle cards without priority normally
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
            Log($"Player {StateMachine.CurrentPlayerId} End Turn");
            ServerManager.Instance.OnPlayerTurnEnded -= OnPlayerTurnEnded;

            StateMachine.CurrentPlayerId++;
            bool isLastPlayer = StateMachine.CurrentPlayerId >= StateMachine.PlayersID.Count;

            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    StateMachine.ChangeState<CheckRitualState>();
                    break;

                case Cycle.Night:
                    if (isLastPlayer)
                        StateMachine.ChangeState<NightConsequencesState>();
                    else
                        StartPlayerTurn(StateMachine.CurrentPlayerId);
                    
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