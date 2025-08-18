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
            Debug.Log("Silenced");
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("silenceActiveCardWasApplied"));
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
