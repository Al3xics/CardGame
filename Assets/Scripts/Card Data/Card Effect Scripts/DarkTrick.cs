using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "DarkTrick", menuName = "Card Effects/DarkTrick")]
    public class DarkTrick : CardEffect
    {
        public GameObject prefabUI;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("darkTrickActiveCardWasApplied"));
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
