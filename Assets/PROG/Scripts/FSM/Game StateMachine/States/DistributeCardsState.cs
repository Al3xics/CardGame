using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// A state within the game state machine that is responsible for distributing cards.
    /// </summary>
    public class DistributeCardsState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state in which cards are distributed within the game state machine.
        /// </summary>
        public DistributeCardsState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnDrawCard += NextState;
            DrawInitialCard(StateMachine.startingActionDeckAmount, StateMachine.startingResourceDeckAmount);
        }

        /// <summary>
        /// Handles the initial drawing of cards for all players by distributing a specified number of action and resource cards.
        /// </summary>
        /// <param name="actionDeckAmount">The number of cards to draw from the action deck for each player.</param>
        /// <param name="resourceDeckAmount">The number of cards to draw from the resource deck for each player.</param>
        private void DrawInitialCard(int actionDeckAmount, int resourceDeckAmount)
        {
            Dictionary<ulong, List<int>> playersCards = new();
            var actionDeck = StateMachine.dataCollectionScript.RuntimeActionDeck;
            var resourceDeck = StateMachine.dataCollectionScript.RuntimeResourcesDeck;
            
            // Calculate total cards needed for all players
            int totalActionCardsNeeded = actionDeckAmount * StateMachine.PlayersID.Count();
            int totalResourceCardsNeeded = resourceDeckAmount * StateMachine.PlayersID.Count();
            
            // Verify if we have enough cards
            if (actionDeck.Count < totalActionCardsNeeded)
                throw new System.Exception($"Not enough action cards in deck! Need {totalActionCardsNeeded} but only have {actionDeck.Count}");
            if (resourceDeck.Count < totalResourceCardsNeeded)
                throw new System.Exception($"Not enough resource cards in deck! Need {totalResourceCardsNeeded} but only have {resourceDeck.Count}");
            
            foreach (var playerID in StateMachine.PlayersID)
            {
                playersCards[playerID] = new List<int>();

                // Action Deck
                int actionCardsToDraw = Mathf.Min(actionDeckAmount, actionDeck.Count);
                for (var i = 0; i < actionCardsToDraw; i++)
                {
                    int randomIndex = Random.Range(0, actionDeck.Count);
                    int cardID = actionDeck[randomIndex].ID;
                    playersCards[playerID].Add(cardID);
                    actionDeck.RemoveAt(randomIndex);
                }
                
                // Resource Deck
                int resourceCardsToDraw = Mathf.Min(resourceDeckAmount, resourceDeck.Count);
                for (var i = 0; i < resourceCardsToDraw; i++)
                {
                    int randomIndex = Random.Range(0, resourceDeck.Count);
                    int cardID = resourceDeck[randomIndex].ID;
                    playersCards[playerID].Add(cardID);
                    resourceDeck.RemoveAt(randomIndex);
                }
            }

            Utils.DictionaryToArrays(playersCards, out ulong[] targets, out int[][] cardsID);
            ServerManager.Instance.SendCardsToPlayersServerRpc(targets, cardsID);
        }

        /// <summary>
        /// Advances the state of the game from the current <see cref="DistributeCardsState"/> to the <see cref="PlayerTurnState"/>.
        /// </summary>
        private void NextState()
        {
            ServerManager.Instance.OnDrawCard -= NextState;
            StateMachine.ChangeState<PlayerTurnState>();
        }
    }
}