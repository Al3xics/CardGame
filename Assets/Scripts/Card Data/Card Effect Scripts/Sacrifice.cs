using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Sacrifice", menuName = "Card Effects/Sacrifice")]
    public class Sacrifice : CardEffect
    {
        [HideInInspector]
        public GameObject prefabUI;
        private ulong originalPlayerID;
        private PlayerController playerController;

        public override async void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            playerController = targetPlayer;
            originalPlayerID = owner;
            if (targetPlayer != null)
            {
                ServerManager.Instance.AskChangeGuardianStatusRpc(true, target, (int)owner);
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("sacrificeActiveCardWasApplied"));
                await UniTask.WaitForSeconds(0.5f);
                targetPlayer.hasGuardian.OnValueChanged += RemoveSacrifice;
            }
        }

        public override bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;

            AnalyticsManager.Instance.RecordEvent(new CustomEvent("trapPassiveCardWasApplied"));
            return true;
        }

        public void RemoveSacrifice(bool oldGuardianStatus, bool newGuardianStatus)
        {
            var player = PlayerController.GetPlayer(originalPlayerID);
            HandManager handManager = player._handManager;
            handManager.DestroyPassiveCard("Sacrifice");
        }



        public override void ShowUI(GameObject uiInstance = null)
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            base.ShowUI(prefabUI);
            prefabUI.SetActive(true);
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
