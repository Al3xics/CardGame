using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "RevealNightActions", menuName = "Card Effects/RevealNightActions")]
    public class RevealNightActions : CardEffect
    {
        public GameObject prefabUI;
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var nightActions = GameStateMachine.Instance.NightActions;
            List<PlayerAction> playerActions = new List<PlayerAction>();
            for (int i = 0; i < nightActions.Count; i++)
            {
                if (nightActions[i].OriginId == target)
                {
                    playerActions.Add(nightActions[i]);
                }
            }
            // Afficher playerActions
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("revealNightActionsActiveCardWasApplied"));
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
