using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "DestructionTrap", menuName = "Card Effects/Destruction Trap")]
    public class DestructionTrap : CardEffect
    {
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            ServerManager.Instance.AskToDestructTrapsRpc();
        }
    }
}
