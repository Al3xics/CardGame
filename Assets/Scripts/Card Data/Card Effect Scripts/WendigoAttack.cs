using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Unity.Services.Analytics;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendigoAttack", menuName = "Card Effects/Wendigo Attack")]
    public class WendigoAttack : CardEffect
    {
        public int damageDone = 1;
        
        public GameObject prefabUI;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            if (value <= -1) value = 0;
            var newValue = damageDone + value;
            
            var targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("wendigoAttackActiveCardWasApplied"));
                
                if (!targetPlayer.hasGuardian.Value && !targetPlayer.isFlighting)
                {
                    targetPlayer.ChangeHealth(newValue);
                }
                else
                {
                    targetPlayer.guardian.ChangeHealth(newValue);
                    targetPlayer.hasGuardian.Value = false;
                }
            }
            
            HandManager handManager = FindFirstObjectByType<HandManager>();
            handManager.DestroyPassiveCard("Trap");
        }

        public override void ShowUI()
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(true);
        }

        public override void HideUI()
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(false);
        }
    }
}