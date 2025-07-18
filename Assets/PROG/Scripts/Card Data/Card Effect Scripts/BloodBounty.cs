using System.Collections.Generic;
using Unity.Services.Analytics;
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
            PlayerController targetPlayer = PlayerController.GetPlayer(target);
            
            if (card.CardEffect is WendigoAttack || (card.CardEffect is FightOrFlight && originPlayer.Role.Value == RoleType.Wendogo) || card.CardEffect is GroupAttack)
            {
                if (!targetPlayer.IsSimulatingNight)
                {
                    if (!targetPlayer.hasGardian.Value)
                    {
                        targetPlayer.health.Value -= boostDamage;
                    }
                    else
                    {
                        targetPlayer.gardian.health.Value -= boostDamage;
                    }
                }
                else if (targetPlayer.IsSimulatingNight)
                {
                    if (!targetPlayer.hasGardian.Value)
                    {
                        targetPlayer.hiddenHealth -= boostDamage;
                    }
                    else
                    {
                        targetPlayer.gardian.hiddenHealth -= boostDamage;
                    }
                }
                
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("bloodBountyPassiveCardWasApplied"));
                return true;
            }
            
            value = -1;
            return false;
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