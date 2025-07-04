using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "GroupHeal", menuName = "Card Effects/Group Heal")]
    public class GroupHeal : CardEffect
    {
        public int healValue = 2;
        
        public GameObject prefabUI;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                targetPlayer.health.Value += healValue;
            }
        }

        public override void ShowUI()
        {
            prefabUI.SetActive(true);
        }
        
        public override void HideUI()
        {
            prefabUI.SetActive(false);
        }
    }
}
