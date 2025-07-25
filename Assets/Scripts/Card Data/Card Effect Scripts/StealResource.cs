using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealResource", menuName = "Card Effects/Steal Resource")]
    public class StealResource : CardEffect
    {
        public int ResourceAmount = 1;
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            Debug.Log($"Stealing resource from {target} by {owner}.");
            if (value != 1000)
            {
                
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("stealResourceActiveCardWasApplied"));
            }
        }
    }
}
