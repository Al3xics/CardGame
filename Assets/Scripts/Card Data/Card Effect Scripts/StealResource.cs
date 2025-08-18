using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealResource", menuName = "Card Effects/Steal Resource")]
    public class StealResource : CardEffect
    {
        public int ResourceAmount = 1;
        public GameObject prefabUITarget;
        public GameObject prefabRessource;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            Debug.Log($"Stealing resource from {target} by {owner}.");

            PlayerController ownerPlayer = PlayerController.GetPlayer(owner);
            PlayerController targetPlayer = PlayerController.GetPlayer(target);

            if (value != 1000)
            {
                //temp
                //Change you can pick the resource
                value = Random.Range(0, 2);
                if (value == 0) // steal wood
                {
                    ServerManager.Instance.ChangePlayerResourceRpc(value, -ResourceAmount, target);
                    ServerManager.Instance.ChangePlayerResourceRpc(value, ResourceAmount, owner);
                }
                else if (value == 1) // steal food
                {
                    ServerManager.Instance.ChangePlayerResourceRpc(value, -ResourceAmount, target);
                    ServerManager.Instance.ChangePlayerResourceRpc(value, ResourceAmount, owner);
                }
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("stealResourceActiveCardWasApplied"));
            }
        }


        public override void ShowUI()
        {
            if (prefabUITarget == null)
                prefabUITarget = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUITarget.SetActive(true);
        }

        public override void HideUI(bool clearVote)
        {
            if (prefabUITarget == null)
                prefabUITarget = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUITarget.SetActive(false);
        }

    }
}

