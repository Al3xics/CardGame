using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "GroupAttack", menuName = "Card Effects/Group Attack")]
    public class GroupAttack : CardEffect
    {
        public int attackValue = 2;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                targetPlayer.health.Value -= attackValue;
            }
        }

        
        public override void ShowUI()
        {
            ServerManager.Instance.UseAllUIForVotersRpc(true);
        }
        
        public override void HideUI()
        {
            ServerManager.Instance.UseAllUIForVotersRpc(false);
        }
    }
}