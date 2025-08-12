using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "RevealNightActions", menuName = "Card Effects/RevealNightActions")]
    public class RevealNightActions : CardEffect
    {
        public GameObject prefabUI;
        private GameObject _showingCardsUI; 
        public override async void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            List<PlayerAction> playerActions = new List<PlayerAction>();
            var serverActions = ServerManager.Instance.nightActions;

            foreach (var action in serverActions)
            {
                if (action.OriginId == target)
                {
                    playerActions.Add(action);
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
