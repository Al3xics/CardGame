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
        /// For the Day:
        /// Check if the card played does an action on another player, and the other player has a passive card to block it.
        /// If it does, Apply the passive card, then apply the card played by the first player. At the end, draw and send a card
        /// to the player to complete his hand.
        ///
        /// For the Night:
        /// 
        /// </summary>
        /// <param name="playedCardID">The card ID played by the player <c>origin</c>.</param>
        /// <param name="origin">The ID of the player using the card.</param>
        /// <param name="target">The targeted ID player.</param>
        /// <exception cref="ArgumentOutOfRangeException">The cycle messed up, and is neither <see cref="Cycle.Day"/>, nor <see cref="Cycle.Night"/></exception>
        public void CheckCardPlayed(int playedCardID, ulong origin, ulong target)
        {
            /*
             * Si un player per de la vie, on ajoute un booléan à un dictionnaire avec la key l'id du player, et la value sa vie
             * 
             */
            
            switch (StateMachine.Cycle)
            {
                case Cycle.Day:
                    // We retrieve the CarteDataSO using the ID, in DataCollectionScript
                    CardDataSO card = StateMachine.dataCollectionScript.cardDatabase.GetCardByID(playedCardID);
                    
                    ServerManager.Instance.TryApplyPassiveServerRpc(playedCardID, origin, target);
                    break;
                
                case Cycle.Night:
                    ulong currentPlayer = StateMachine.PlayersID[StateMachine.CurrentPlayerId];

                    if (!StateMachine.NightActions.ContainsKey(currentPlayer))
                        StateMachine.NightActions[currentPlayer] = new List<PlayerAction>();

                    StateMachine.NightActions[currentPlayer].Add(new PlayerAction
                    {
                        CardId = playedCardID,
                        OriginId = origin,
                        TargetId = target
                    });

                    ServerManager.Instance.FinishedCheckCardPlayedServerRpc(origin);
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