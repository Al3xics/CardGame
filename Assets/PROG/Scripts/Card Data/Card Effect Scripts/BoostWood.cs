using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BoostWood", menuName = "Card Effects/Boost Wood")]
    public class BoostWood : CardEffect
    {
        public int boostWoodBy = 1;
        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId);
            
            if (card.CardEffect is ScavengeWood)
            {
                value = boostWoodBy;
                Debug.Log($"BoostWood passive applied! Boost by {value}");
                return true;
            }
            
            return false;
        }
    }
}
