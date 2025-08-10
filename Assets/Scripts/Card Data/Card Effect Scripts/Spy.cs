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
        public async override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);

            if (targetPlayer != null && targetPlayer.PassiveCards != null && targetPlayer.PassiveCards.Count != 0)
            {
                int index = Random.Range(0, targetPlayer.PassiveCards.Count);
                int selectedCard = targetPlayer.PassiveCards[index];

                value = selectedCard;
                await ShowPlayerActions(value);
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("spyActiveCardWasApplied"));
            }

        }

        private async UniTask ShowPlayerActions(int CardId)
        {
            _showingCardsUI = Instantiate(showingPrefabUI);
            _showingCardsUI.SetActive(true);
            RawImage imageToChange = _showingCardsUI.GetComponentInChildren<RawImage>();

                CardDataSO cardDataSO = DataCollection.Instance.cardDatabase.GetCardByID(CardId);
                Texture2D texture = cardDataSO.CardVisual;
                imageToChange.texture = texture;
                await UniTask.WaitForSeconds(3);
            
            _showingCardsUI.SetActive(false);
            Destroy(_showingCardsUI);
        }

        public override void ShowUI()
        {
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
