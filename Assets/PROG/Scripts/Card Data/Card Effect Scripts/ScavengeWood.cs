using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "ScavengeWood", menuName = "Card Effects/Scavenge Wood")]
    public class ScavengeWood : CardEffect
    {
        public int woodGained = 1;

        public override void Apply(ulong owner, ulong target, int value = default)
        {
            PlayerController.GetPlayer(target).wood.Value += woodGained;
        }
    }
}
