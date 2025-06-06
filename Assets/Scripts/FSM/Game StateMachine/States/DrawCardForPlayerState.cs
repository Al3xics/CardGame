using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    public class DrawCardForPlayerState : State<GameStateMachine>
    {
        public DrawCardForPlayerState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnDrawCard += NextState;
        }

        public void DrawCards(ulong playerID, int deckID, int amount)
        {
            Dictionary<ulong, List<int>> playerCards = new();
            var deck = StateMachine.DataCollectionScript.GetDeck(deckID);
            
            playerCards[playerID] = new List<int>();
            
            int cardsToDraw = Mathf.Min(amount, deck.Count);
            for (var i = 0; i < cardsToDraw; i++)
            {
                var randomIndex = Random.Range(0, deck.Count);
                var card = deck[randomIndex];
                int cardID = StateMachine.DataCollectionScript.GetCardID(card);
                playerCards[playerID].Add(cardID);
                deck.RemoveAt(randomIndex);
            }

            ServerManager.Instance.SendCardsToPlayers(playerCards);
        }

        private void NextState()
        {
            ServerManager.Instance.OnDrawCard -= NextState;
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}