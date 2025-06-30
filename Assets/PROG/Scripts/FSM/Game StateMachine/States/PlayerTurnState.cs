using System;
using System.Collections.Generic;
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
            var card = StateMachine.dataCollectionScript.cardDatabase.GetCardByID(playedCardID);
            
            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    // For passive cards
                    if (card.isPassive)
                    {
                        PlayerController.GetPlayer(origin).PassiveCards.Add(card);
                        ServerManager.Instance.FinishedCheckCardPlayedServerRpc(origin);
                        return;
                    }
                    
                    // For active cards
                    ServerManager.Instance.TryApplyPassiveServerRpc(playedCardID, origin, target);
                    break;
                
                case Cycle.Night:
                    // For passive cards
                    if (card.isPassive)
                    {
                        PlayerController.GetPlayer(origin).HiddenPassiveCards.Add(card);
                        ServerManager.Instance.FinishedCheckCardPlayedServerRpc(origin);
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
                        ServerManager.Instance.TryApplyPassiveServerRpc(playedCardID, origin, target);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(StateMachine.Cycle), StateMachine.Cycle, "The cycle is not valid.");
            }
        }

        /// <summary>
        /// Handles the result of a passive card effect in the game, applying the card's effect
        /// based on the provided parameters and notifying the server upon completion.
        /// </summary>
        /// <param name="playedCardId">The ID of the card whose passive effect is being processed.</param>
        /// <param name="origin">The identifier of the player or entity that played the card.</param>
        /// <param name="target">The identifier of the player or entity targeted by the card effect.</param>
        /// <param name="isApply">Indicates whether the effect should be applied or reverted.</param>
        /// <param name="value">The value associated with the effect being applied.</param>
        public void OnPassiveResultReceived(int playedCardId, ulong origin, ulong target, bool isApply, int value)
        {
            var effect = StateMachine.dataCollectionScript.cardDatabase.GetCardByID(playedCardId).CardEffect;

            effect.Apply(origin, target, isApply ? value : -1);
            ServerManager.Instance.FinishedCheckCardPlayedServerRpc(origin);
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