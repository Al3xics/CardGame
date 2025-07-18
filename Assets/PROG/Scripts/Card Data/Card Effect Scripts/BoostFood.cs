using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BoostFood", menuName = "Card Effects/Boost Food")]
    public class BoostFood : CardEffect
    {
        public int boostFoodBy = 1;

        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId);
            
            if (card.CardEffect is ScavengeFood)
            {
                value = boostFoodBy;
                Debug.Log($"BoostFood passive applied! Boost by {value}");
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("boostFoodPassiveCardWasApplied"));
                return true;
            }
            
            return false;
        }
    }
}