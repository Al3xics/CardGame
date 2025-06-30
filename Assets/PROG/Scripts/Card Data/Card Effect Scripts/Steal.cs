using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Steal", menuName = "Card Effects/Steal")]
    public class Steal : CardEffect
    {
        public int typeOfRessource = 0;

        public override void Apply(ulong owner, ulong target, int value = default)
        {
            if(typeOfRessource == 0)
            {
                PlayerController.GetPlayer(owner).food.Value += 1;
                PlayerController.GetPlayer(target).food.Value -= 1;
            } 
            else
            {
                PlayerController.GetPlayer(owner).wood.Value += 1;
                PlayerController.GetPlayer(target).wood.Value -= 1;
            }
        }

    }
}