using UnityEngine;
using UnityEngine.Serialization;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Trap", menuName = "Card Effects/Trap")]
    public class Trap : CardEffect
    {
        public int defenseValue = 1;

        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            var card = DataCollection.Instance.cardDatabase.GetCardByID(playedCardId);
            
            PlayerController originPlayer = PlayerController.GetPlayer(origin);
            
            if (card.CardEffect is WendigoAttack || (card.CardEffect is FightOrFlight && originPlayer.Role.Value == RoleType.Wendogo) || card.CardEffect is GroupAttack)
            {
                value = defenseValue;
                Debug.Log($"Defense by {value}");
                return true;
            }
            
            value = -1;
            return false;
        }
    }
}
