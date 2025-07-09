using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Sacrifice", menuName = "Card Effects/Sacrifice")]
    public class Sacrifice : CardEffect
    {
        [HideInInspector]
        public GameObject prefabUI;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                targetPlayer.asGardian.Value = true;
            }
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
