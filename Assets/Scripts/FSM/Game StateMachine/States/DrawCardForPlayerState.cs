using System.Collections.Generic;
using System.Linq;
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
                var randomKey = deck.Keys.ElementAt(Random.Range(0, deck.Count));

                playerCards[playerID].Add(randomKey);

                deck.Remove(randomKey);
            }

            Utils.DictionaryToArrays(playerCards, out ulong[] targets, out int[][] cardsID);

            ServerManager.Instance.SendCardsToPlayersServerRpc(targets, cardsID);
        }

        private void NextState()
        {
            ServerManager.Instance.OnDrawCard -= NextState;
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}