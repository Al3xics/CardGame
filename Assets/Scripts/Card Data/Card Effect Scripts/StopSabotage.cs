using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StopSabotage", menuName = "Card Effects/Stop Sabotage")]
    public class StopSabotage : CardEffect
    {
        public GameObject prefabUI;
        private GameObject _stopCanvaInstance;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            if (target == 0)
                ServerManager.Instance.AskToUnlockResourcesRpc(false, false);
            else if (target == 1)
                ServerManager.Instance.AskToUnlockResourcesRpc(true, false);
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("stopSabotageActiveCardWasApplied"));
        }
        
        
        public override void ShowUI(GameObject uiInstance = null)
        {
            if (_stopCanvaInstance == null)
                _stopCanvaInstance = Instantiate(prefabUI);
            base.ShowUI(_stopCanvaInstance);
            _stopCanvaInstance.SetActive(true);
        }
        
        public override void HideUI(bool clearVotes, GameObject uiInstance = null)
        {
            if (prefabUI == null)
                _stopCanvaInstance.SetActive(false);
            Destroy(_stopCanvaInstance.gameObject);
            base.HideUI(clearVotes, _stopCanvaInstance);
        }
    }
}
