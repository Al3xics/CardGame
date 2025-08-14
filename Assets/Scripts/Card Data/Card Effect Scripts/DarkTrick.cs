using Unity.Services.Analytics;
using UnityEngine;

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

        public override void ShowUI()
        {
            if (PlayerController.LocalPlayer.Role.Value == RoleType.Wendogo)
            {
                if (_dtCanvaInstance == null)
                    _dtCanvaInstance = Instantiate(WendogoTrickprefabUI);
                _dtCanvaInstance.SetActive(true);
            }
            else
            {
                if (_dtCanvaInstance == null)
                    _dtCanvaInstance = Instantiate(SelectTargetPrefabUI);
                _dtCanvaInstance.SetActive(true);
            }
        }

        public override void HideUI(bool clearVotes)
        {
            _dtCanvaInstance.SetActive(false);
            Destroy(_dtCanvaInstance.gameObject);
            base.HideUI(clearVotes);
        }
    }
}
