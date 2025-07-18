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
            var targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("wendigoAttackActiveCardWasApplied"));
                
                if (!targetPlayer.hasGardian.Value)
                {
                    targetPlayer.ChangeHealth(-damageDone);
                }
                else
                {
                    targetPlayer.gardian.ChangeHealth(-damageDone);
                    targetPlayer.hasGardian.Value = false;
                }
            }
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