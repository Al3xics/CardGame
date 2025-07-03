using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealResourceEffect", menuName = "Card Effects/Steal Resource")]
    public class StealResourceEffect : CardEffect
    {
        public int ResourceAmount = 1;
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            Debug.Log($"Stealing resource from {target} by {owner}.");
            if (value == -1)
            {
                
            }
            else
            {
                
            }
        }
    }
}
