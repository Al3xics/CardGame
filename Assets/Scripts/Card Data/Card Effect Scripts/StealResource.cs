using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealResource", menuName = "Card Effects/Steal Resource")]
    public class StealResource : CardEffect
    {
        public int ResourceAmount = 1;
        public GameObject prefabUI;
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            Debug.Log($"Stealing resource from {target} by {owner}.");
            
            PlayerController ownerPlayer = PlayerController.GetPlayer(owner);
            PlayerController targetPlayer = PlayerController.GetPlayer(target);
            
            if (value != 1000)
            {
                if (value == 0) // steal food
                {
                    if (targetPlayer.IsSimulatingNight)
                    {
                        targetPlayer.hiddenFood -= ResourceAmount;
                        ownerPlayer.hiddenFood += ResourceAmount;
                    }
                    else
                    {
                        targetPlayer.food.Value -= ResourceAmount;
                        ownerPlayer.food.Value += ResourceAmount;
                    }
                }
                else if (value == 1) // steal wood
                {
                    if (targetPlayer.IsSimulatingNight)
                    {
                        targetPlayer.hiddenWood -= ResourceAmount;
                        ownerPlayer.hiddenWood += ResourceAmount;
                    }
                    else
                    {
                        targetPlayer.wood.Value -= ResourceAmount;
                        ownerPlayer.wood.Value += ResourceAmount;
                    }
                }
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("stealResourceActiveCardWasApplied"));
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
