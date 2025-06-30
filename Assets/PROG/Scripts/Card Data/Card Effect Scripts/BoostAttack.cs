using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BoostAttack", menuName = "Card Effects/Boost Attack")]
    public class BoostAttack : CardEffect
    {
        public int boostAttackBy = 1;

        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            DataCollection script = GameObject.Find("DataCollection").GetComponent<DataCollection>();
            CardDataSO card = script.cardDatabase.GetCardByID(playedCardId);
            
            PlayerController originPlayer = PlayerController.GetPlayer(origin);
            
            if (card.CardEffect is WendigoAttack || (card.CardEffect is FightOrFlight && originPlayer.Role.Value == RoleType.Wendogo) || card.CardEffect is GroupAttack)
            {
                value = boostAttackBy;
                Debug.Log($"Boost attack by {value}");
                return true;
            }
            
            value = -1;
            return false;
        }
    }
}
