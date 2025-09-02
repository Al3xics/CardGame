using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Spy", menuName = "Card Effects/Spy")]
    public class Spy : CardEffect
    {
        public GameObject prefabUI;
        private GameObject targetPrefabInstance;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            var ownerPlayer = PlayerController.GetPlayer(owner);

            if (targetPlayer != null && targetPlayer.PassiveCards != null && targetPlayer.PassiveCards.Count != 0)
            {
                int index = Random.Range(0, targetPlayer.PassiveCards.Count);
                int selectedCard = targetPlayer.PassiveCards[index];

                value = selectedCard;

                ServerManager.Instance.ShowCardsCanvaRpc(selectedCard, owner); ;
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("spyActiveCardWasApplied"));
            }

        }

        public override void ShowUI(GameObject uiInstance = null)
        {
            if (targetPrefabInstance == null)
                targetPrefabInstance = Instantiate(prefabUI);
            base.ShowUI(targetPrefabInstance);
            targetPrefabInstance.SetActive(true);
            ServerManager.GlobalPlayersByName.TryGetValue(0, out var value1);
            ServerManager.GlobalPlayersByName.TryGetValue(1, out var value2);
            ServerManager.GlobalPlayersByName.TryGetValue(2, out var value3);
            ServerManager.GlobalPlayersByName.TryGetValue(3, out var value4);
            targetPrefabInstance.transform.Find("Panel").transform.Find("ButtonPlayer1").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value1;
            targetPrefabInstance.transform.Find("Panel").transform.Find("ButtonPlayer2").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value2;
            targetPrefabInstance.transform.Find("Panel").transform.Find("ButtonPlayer3").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value3;
            targetPrefabInstance.transform.Find("Panel").transform.Find("ButtonPlayer4").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value4;
        }

        public override void HideUI(bool clearVotes, GameObject uiInstance = null)
        {
            targetPrefabInstance.SetActive(false);
            Destroy(targetPrefabInstance.gameObject);
            base.HideUI(clearVotes, targetPrefabInstance);
        }
    }
}
