using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Spy", menuName = "Card Effects/Spy")]
    public class Spy : CardEffect
    {
        [HideInInspector]
        public GameObject prefabUI;
        public GameObject showingPrefabUI;
        private GameObject _showingCardsUI;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            var ownerPlayer = PlayerController.GetPlayer(owner);

            if (targetPlayer != null && targetPlayer.PassiveCards != null && targetPlayer.PassiveCards.Count != 0)
            {
                int index = Random.Range(0, targetPlayer.PassiveCards.Count);
                int selectedCard = targetPlayer.PassiveCards[index];

                value = selectedCard;
                
                ServerManager.Instance.ShowCardsCanvaRpc(selectedCard, owner);;
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("spyActiveCardWasApplied"));
            }

        }

        public override void ShowUI()
        {
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
