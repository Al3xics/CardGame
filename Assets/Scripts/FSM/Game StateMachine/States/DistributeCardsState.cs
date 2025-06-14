using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wendogo
{
    public class DistributeCardsState : State<GameStateMachine>
    {
        public DistributeCardsState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnDrawCard += NextState;
            DrawInitialCard(StateMachine.startingActionDeckAmount, StateMachine.startingResourceDeckAmount);
        }

        private void DrawInitialCard(int actionDeckAmount, int resourceDeckAmount)
        {
            /*
             * Dictionnaire, <playerID, List<int>>
             * 
             * Pour chaque player dans PlayersID on fera :
             * 
             * On récupère X cartes dans deck 1 --> on la choisie aléatoirement dans le deck, on récupère l'id de la carte, on l'ajoute au dictionnaire, puis on la supprime du deck
             * On récupère X cartes dans deck 2 --> on la choisie aléatoirement dans le deck, on récupère l'id de la carte, on l'ajoute au dictionnaire, puis on la supprime du deck
             *
             * On envoie le dico au ServerManager
             */

            Dictionary<ulong, List<int>> playersCards = new();
            var actionDeck = StateMachine.dataCollectionScript.actionDeck.CardsDeck;
            var resourceDeck = StateMachine.dataCollectionScript.resourcesDeck.CardsDeck;
            
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

        private void NextState()
        {
            ServerManager.Instance.OnDrawCard -= NextState;
            StateMachine.ChangeState<PlayerTurnState>();
        }
    }
}