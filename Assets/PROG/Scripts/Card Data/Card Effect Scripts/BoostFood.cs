using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BoostFood", menuName = "Card Effects/Boost Food")]
    public class BoostFood : CardEffect
    {
        public int boostFoodBy = 1;

        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            DataCollection script = GameObject.Find("DataCollection").GetComponent<DataCollection>();
            CardDataSO card = script.cardDatabase.GetCardByID(playedCardId);
            
            if (card.CardEffect is ScavengeFood)
            {
                value = boostFoodBy;
                Debug.Log($"Boost food by {value}");
                return true;
            }
            
            value = -1;
            return false;
        }
    }
}