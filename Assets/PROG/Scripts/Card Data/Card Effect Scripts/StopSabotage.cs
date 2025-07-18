using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StopSabotage", menuName = "Card Effects/Stop Sabotage")]
    public class StopSabotage : CardEffect
    {
        public GameObject prefabUI;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            if (value == 0)
                ServerManager.Instance.AskToUnlockRessoucesRpc(true, false);
            else if (value == 1)
                ServerManager.Instance.AskToUnlockRessoucesRpc(false, false);
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("stopSabotageActiveCardWasApplied"));
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
