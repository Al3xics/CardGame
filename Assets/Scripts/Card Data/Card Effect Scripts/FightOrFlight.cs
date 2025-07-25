using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "FightOrFlight", menuName = "Card Effects/Fight Or Flight")]
    public class FightOrFlight : CardEffect
    {
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("fightOrFlightActiveCardWasApplied"));
        }
    }
}
