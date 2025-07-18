using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Spy", menuName = "Card Effects/Spy")]
    public class Spy : CardEffect
    {
        [HideInInspector]
        public GameObject prefabUI;
        
        [HideInInspector]
        public GameObject prefabShowCard;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);

            if (targetPlayer != null && targetPlayer.PassiveCards != null && targetPlayer.PassiveCards.Count != 0)
            {
                int index = Random.Range(0, targetPlayer.PassiveCards.Count);
                int selectedCard = targetPlayer.PassiveCards[index];
                
                value = selectedCard;
                
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("spyActiveCardWasApplied"));
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
