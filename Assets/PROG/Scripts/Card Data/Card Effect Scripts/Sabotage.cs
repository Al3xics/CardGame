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
            
            if (value == 0)
            {
                // Sabotage Food
            } 
            else if (value == 1)
            {
                // Sabotage Wood
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
