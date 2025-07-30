using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Silence", menuName = "Card Effects/Silence")]
    public class Silence : CardEffect
    {
        public GameObject prefabUI;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            ServerManager.Instance.MutePlayerRpc(true, target);
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("silenceActiveCardWasApplied"));
        }
        
        public override void ShowUI()
        {

        }

        public override void HideUI()
        {

        }
    }
}
