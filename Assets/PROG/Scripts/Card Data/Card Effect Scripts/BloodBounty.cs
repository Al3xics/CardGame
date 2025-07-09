using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BloodBounty", menuName = "Card Effects/Blood Bounty")]
    public class BloodBounty : CardEffect
    {
        public GameObject SelectResourcePrefab;
        
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

        public void ApplyRitualEffect(ulong origin, List<int> resourceList)
        {
            var player = PlayerController.GetPlayer(origin);

            foreach (var resource in resourceList)
            {
                if (resource == 0 && player.IsSimulatingNight) //food
                {
                    player.hiddenFood += 1;
                }
                else if (resource == 1 && player.IsSimulatingNight) //wood
                {
                    player.hiddenWood += 1;
                }
                else if (resource == 0 && !player.IsSimulatingNight) //food
                {
                    player.food.Value += 1;
                }
                else if (resource == 1 && !player.IsSimulatingNight) //wood
                {
                    player.wood.Value += 1;
                }
            }
        }
        
        public override void ShowUI()
        {
            SelectResourcePrefab.SetActive(true);
        }
        
        public override void HideUI()
        {
            SelectResourcePrefab.SetActive(false);
        }
    }
}