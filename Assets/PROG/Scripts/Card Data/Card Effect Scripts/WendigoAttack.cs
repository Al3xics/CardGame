using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendigoAttack", menuName = "Card Effects/Wendigo Attack")]
    public class WendigoAttack : CardEffect
    {
        public int damageDone = 1;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            
        }
    }
}
