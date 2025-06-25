using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "ScavengeFood", menuName = "Card Effects/Scavenge Food")]
    public class ScavengeFood : CardEffect
    {

        public int foodGained = 1;

        public override void Apply(ulong owner, ulong target, int value = default)
        {
            if (target == 0)
            {
                ServerManager.Instance.player1Food.Value += foodGained;
            }
            else if (target == 1)
            {
                ServerManager.Instance.player2Food.Value += foodGained;
            }
            else if (target == 2)
            {
                ServerManager.Instance.player3Food.Value += foodGained;
            }
            else
            {
                ServerManager.Instance.player4Food.Value += foodGained;
            }
        }
    }
}
