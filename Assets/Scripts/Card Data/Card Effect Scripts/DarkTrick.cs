using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "DarkTrick", menuName = "Card Effects/DarkTrick")]
    public class DarkTrick : CardEffect
    {
        public GameObject WendogoTrickprefabUI;
        public GameObject SelectTargetPrefabUI;
        private GameObject _dtCanvaInstance;
        [SerializeField] private CardDataSO _dtRevealCard;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("darkTrickActiveCardWasApplied"));
        }

        public override void ShowUI(GameObject uiInstance = null)
        {
            if (PlayerController.LocalPlayer.Role.Value == RoleType.Wendogo)
            {
                if (_dtCanvaInstance == null)
                    _dtCanvaInstance = Instantiate(WendogoTrickprefabUI);
                base.ShowUI(_dtCanvaInstance);
                _dtCanvaInstance.SetActive(true);
                
                
            }
            else
            {
                if (_dtCanvaInstance == null)
                    _dtCanvaInstance = Instantiate(SelectTargetPrefabUI);
                base.ShowUI(_dtCanvaInstance);
                _dtCanvaInstance.SetActive(true);
            }
            
            ServerManager.GlobalPlayersByName.TryGetValue(0, out var value1);
            ServerManager.GlobalPlayersByName.TryGetValue(1, out var value2);
            ServerManager.GlobalPlayersByName.TryGetValue(2, out var value3);
            ServerManager.GlobalPlayersByName.TryGetValue(3, out var value4);
            
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer1").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value1;
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer2").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value2;
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer3").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value3;
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer4").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value4;
            
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer1").GetComponent<Image>().sprite = PlayerUI.Instance.PlayerSpritesIcons[0];
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer2").GetComponent<Image>().sprite = PlayerUI.Instance.PlayerSpritesIcons[1];
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer3").GetComponent<Image>().sprite = PlayerUI.Instance.PlayerSpritesIcons[2];
            _dtCanvaInstance.transform.Find("UpdatedSelectTargetCanvas").transform.Find("Panel").transform.Find("ButtonPlayer4").GetComponent<Image>().sprite = PlayerUI.Instance.PlayerSpritesIcons[3];
        }

        public override void HideUI(bool clearVotes, GameObject uiInstance = null)
        {
            _dtCanvaInstance.SetActive(false);
            Destroy(_dtCanvaInstance.gameObject);
            base.HideUI(clearVotes, _dtCanvaInstance);
        }
    }
}
