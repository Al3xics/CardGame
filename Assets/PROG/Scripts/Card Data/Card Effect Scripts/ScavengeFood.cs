using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "ScavengeFood", menuName = "Card Effects/Scavenge Food")]
    public class ScavengeFood : CardEffect
    {
        public int foodGained = 1;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            // If 'value' is '-1', then no value was passed and just use foodGained
            int food = value != -1 ? value + foodGained : foodGained;
            
            Debug.Log($"Scavenge food by {food}");
            
            if (target == 0)
                ServerManager.Instance.player1Food.Value += food;
            else if (target == 1)
                ServerManager.Instance.player2Food.Value += food;
            else if (target == 2)
                ServerManager.Instance.player3Food.Value += food;
            else if (target == 3)
                ServerManager.Instance.player4Food.Value += food;
        }
    }
}
