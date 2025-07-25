using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BoostAttack", menuName = "Card Effects/Boost Attack")]
    public class BoostAttack : CardEffect
    {
        public int boostAttackBy = 1;

        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId);

            PlayerController originPlayer = PlayerController.GetPlayer(origin);

            if (card.CardEffect is WendigoAttack ||
                (card.CardEffect is FightOrFlight && originPlayer.Role.Value == RoleType.Wendogo) ||
                card.CardEffect is GroupAttack)
            {
                value = boostAttackBy;
                Debug.Log($"Defense by {value}");
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("boostAttackPassiveCardWasApplied"));
                return true;
            }

            value = -1;
            return false;
        }
    }
}
