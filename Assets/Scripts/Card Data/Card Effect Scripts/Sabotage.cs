using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Sabotage", menuName = "Card Effects/Sabotage")]
    public class Sabotage : CardEffect
    {
        public GameObject prefabUI;
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("sabotageActiveCardWasApplied"));

            if (target == 0)
            {
                ServerManager.Instance.AskToUnlockResourcesRpc(false, true);
            } 
            else if (target == 1)
            {
                ServerManager.Instance.AskToUnlockResourcesRpc(true, true);
            }
        }


        public override void ShowUI(GameObject uiInstance = null)
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            base.ShowUI(prefabUI);
            prefabUI.SetActive(true);
        }

        public override void HideUI(bool clearVotes, GameObject uiInstance = null)
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(false);
            base.HideUI(clearVotes, prefabUI);
        }
    }
}
