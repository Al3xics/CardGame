using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealProtect", menuName = "Card Effects/Steal Protect")]
    public class StealProtect : CardEffect
    {
        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId);
            
            if (card.CardEffect is StealResourceEffect)
            {
                value = 1000;
                Debug.Log($"Et non ! C'était du porc !");
                return true;
            }
            return false;
        }
    }
}
