using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "PointAction", menuName = "Card Effects/Point Action")]
    public class PointAction : CardEffect
    {
        public int points = 2;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var player = PlayerController.GetPlayer(target);
            player._playerPA += points;
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("pointActionActiveCardWasApplied"));
        }
    }
}
