using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Analytics;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Divination", menuName = "Card Effects/Divination")]
    public class Divination : CardEffect
    {
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            foreach (var player in ServerManager.Instance.GetAllPlayers())
            {
                RevealRandomCards(player);
            }
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("divinationActiveCardWasApplied"));
        }

        private void RevealRandomCards(PlayerController player)
        { 
            // Sélectionne deux cartes aléatoires
            List<GameObject> hand = new List<GameObject>(player._handManager.handCards);
            System.Random rnd = new System.Random();
            int cardIndex1 = rnd.Next(hand.Count);
            GameObject card1 = hand[cardIndex1];
            hand.RemoveAt(cardIndex1); // Supprime pour garantir la non-redondance

            int cardIndex2 = rnd.Next(hand.Count);
            GameObject card2 = hand[cardIndex2];

            // Prévient tous les joueurs que ces cartes sont révélées
            ServerManager.Instance.RevealCardsRpc(player.OwnerClientId, new List<GameObject> { card1, card2 });
        }
    }
}
