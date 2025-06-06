using System.Collections.Generic;
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
            var actionDeck = StateMachine.DataCollectionScript.ActionDeck;
            var resourceDeck = StateMachine.DataCollectionScript.ResourceDeck;
            
            foreach (var playerID in StateMachine.PlayersID)
            {
                playersCards[playerID] = new List<int>();

                // Action Deck
                int actionCardsToDraw = Mathf.Min(actionDeckAmount, actionDeck.Count);
                for (var i = 0; i < actionCardsToDraw; i++)
                {
                    var randomIndex = Random.Range(0, actionDeck.Count);
                    var card = actionDeck[randomIndex];
                    int cardID = StateMachine.DataCollectionScript.GetCardID(card);
                    playersCards[playerID].Add(cardID);
                    actionDeck.RemoveAt(randomIndex);
                }

                // Resource Deck
                int resourceCardsToDraw = Mathf.Min(resourceDeckAmount, resourceDeck.Count);
                for (var i = 0; i < resourceCardsToDraw; i++)
                {
                    var randomIndex = Random.Range(0, resourceDeck.Count);
                    var card = resourceDeck[randomIndex];
                    int cardID = StateMachine.DataCollectionScript.GetCardID(card);
                    playersCards[playerID].Add(cardID);
                    resourceDeck.RemoveAt(randomIndex);
                }
            }

            ServerManager.Instance.SendCardsToPlayers(playersCards);
        }

        private void NextState()
        {
            ServerManager.Instance.OnDrawCard -= NextState;
            StateMachine.ChangeState<AllocateActionPointsState>();
        }
    }
}