using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Unity.Services.Analytics;
using static UnityEngine.UI.Image;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendigoAttack", menuName = "Card Effects/Wendigo Attack")]
    public class WendigoAttack : CardEffect
    {
        public int damageDone = 1;
        
        public GameObject prefabUI;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            if (value == -1) value = 0;
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
                    ulong guardianID = targetPlayer.guardianID;
                    ServerManager.Instance.ChangePlayerHealthRpc(newValue, guardianID);
                    ServerManager.Instance.AskChangeGuardianStatusRpc(false, target);
                }
            }
        }

        public override void ShowUI()
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(true);
        }

        public override void HideUI(bool clearVotes)
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(false);
            base.HideUI(clearVotes);
        }
    }
}