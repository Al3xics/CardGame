using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StopSabotage", menuName = "Card Effects/Stop Sabotage")]
    public class StopSabotage : CardEffect
    {
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("stopSabotageActiveCardWasApplied"));
        }
    }
}
