using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

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
        public override void ShowUI(GameObject uiInstance = null)
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            base.ShowUI(prefabUI);
            prefabUI.SetActive(true);
            
            ServerManager.GlobalPlayersByName.TryGetValue(0, out var value1);
            ServerManager.GlobalPlayersByName.TryGetValue(1, out var value2);
            ServerManager.GlobalPlayersByName.TryGetValue(2, out var value3);
            ServerManager.GlobalPlayersByName.TryGetValue(3, out var value4);
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer1").transform.Find("Player_2").transform.Find("Player_2_Name").GetComponent<TMP_Text>().text = value1;
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer2").transform.Find("Player_2").transform.Find("Player_2_Name").GetComponent<TMP_Text>().text = value2;
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer3").transform.Find("Player_2").transform.Find("Player_2_Name").GetComponent<TMP_Text>().text = value3;
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer4").transform.Find("Player_2").transform.Find("Player_2_Name").GetComponent<TMP_Text>().text = value4;
                
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer1").transform.Find("Player_2").transform.Find("Player_2_Icon").GetComponent<RawImage>().texture = PlayerUI.Instance.PlayerImageIcons[0];
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer2").transform.Find("Player_2").transform.Find("Player_2_Icon").GetComponent<RawImage>().texture = PlayerUI.Instance.PlayerImageIcons[1];
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer3").transform.Find("Player_2").transform.Find("Player_2_Icon").GetComponent<RawImage>().texture = PlayerUI.Instance.PlayerImageIcons[2];
            prefabUI.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer4").transform.Find("Player_2").transform.Find("Player_2_Icon").GetComponent<RawImage>().texture = PlayerUI.Instance.PlayerImageIcons[3];
        }

        public override void HideUI(bool clearVotes, GameObject uiInstance = null)
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(false);
            base.HideUI(clearVotes, prefabUI);
        }
    }
}
