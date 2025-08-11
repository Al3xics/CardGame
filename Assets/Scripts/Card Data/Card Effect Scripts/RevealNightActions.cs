using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "RevealNightActions", menuName = "Card Effects/RevealNightActions")]
    public class RevealNightActions : CardEffect
    {
        public GameObject prefabUI;
        private GameObject _showingCardsUI; 
        public override async void Apply(ulong owner, ulong target, int value = -1)
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
            foreach (var action in playerActions)
            {
                ServerManager.Instance.ShowCardsCanvaRpc(action.CardId, owner);
                await UniTask.WaitForSeconds(1.5f);
            }

            AnalyticsManager.Instance.RecordEvent(new CustomEvent("revealNightActionsActiveCardWasApplied"));
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
