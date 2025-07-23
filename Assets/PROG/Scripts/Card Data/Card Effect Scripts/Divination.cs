using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Analytics;
using Random = System.Random;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Divination", menuName = "Card Effects/Divination")]
    public class Divination : CardEffect
    {
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            RevealRandomCards();
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("divinationActiveCardWasApplied"));
        }

        private void RevealRandomCards()
        {
            Dictionary<ulong, int[]> dict = new();
            
            foreach (var player in ServerManager.Instance.GetAllPlayers())
            {
                // Sélectionne deux cartes aléatoires
                List<GameObject> hand = new List<GameObject>(player._handManager.handCards);
                Random rnd = new Random();
                int cardIndex1 = rnd.Next(hand.Count);
                GameObject card1 = hand[cardIndex1];
                int card1ID = card1.GetComponent<CardObjectData>().Card.ID;
                hand.RemoveAt(cardIndex1); // Supprime pour garantir la non-redondance

                int cardIndex2 = rnd.Next(hand.Count);
                GameObject card2 = hand[cardIndex2];
                int card2ID = card2.GetComponent<CardObjectData>().Card.ID;
                
                dict.Add(player.OwnerClientId, new int[] {card1ID, card2ID});
            }

            // Prévient tous les joueurs que ces cartes sont révélées
            Utils.DictionaryToArrays(dict, out ulong[] playerID, out int[][] arrayCardsID);
            ServerManager.Instance.RevealCardsRpc(playerID, arrayCardsID);
        }
    }
}
