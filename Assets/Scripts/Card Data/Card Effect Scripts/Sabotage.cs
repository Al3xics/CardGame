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
                GameStateMachine.Instance.OnCycleChanged += ResetWood;
            } 
            else if (target == 1)
            {
                ServerManager.Instance.AskToUnlockResourcesRpc(true, true);
                GameStateMachine.Instance.OnCycleChanged += ResetFood;

            }
        }
        
        private void ResetWood(Cycle cycle)
        {
            ServerManager.Instance.AskToUnlockResourcesRpc(false, false);
            GameStateMachine.Instance.OnCycleChanged -= ResetWood;
        }

        private void ResetFood(Cycle cycle)
        {
            ServerManager.Instance.AskToUnlockResourcesRpc(false, false);

            GameStateMachine.Instance.OnCycleChanged -= ResetFood;
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
