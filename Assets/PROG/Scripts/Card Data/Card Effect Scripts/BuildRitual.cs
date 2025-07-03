using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitual", menuName = "Card Effects/Build Ritual")]
    public class BuildRitual : CardEffect
    {
        public int typeOfRessource = 0;

        public override void Apply(ulong owner, ulong target, int value = default)
        {
            if (typeOfRessource == 0)
            {
                PlayerController.GetPlayer(target).food.Value -= 1;
            }
            else
            {
                PlayerController.GetPlayer(target).wood.Value -= 1;
            }
        }
    }
}
