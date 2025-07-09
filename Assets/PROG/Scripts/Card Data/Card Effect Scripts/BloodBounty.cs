using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BloodBounty", menuName = "Card Effects/Blood Bounty")]
    public class BloodBounty : CardEffect
    {
        public int boostDamage = 1;
        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId);
            
            PlayerController originPlayer = PlayerController.GetPlayer(origin);
            
            if (card.CardEffect is WendigoAttack || (card.CardEffect is FightOrFlight && originPlayer.Role.Value == RoleType.Wendogo) || card.CardEffect is GroupAttack)
            {
                value = boostDamage;
                Debug.Log($"Attack boosted by {value}");
                return true;
            }
            
            value = -1;
            return false;
        }
    }
}