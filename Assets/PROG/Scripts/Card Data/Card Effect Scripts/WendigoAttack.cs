using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendigoAttack", menuName = "Card Effects/Wendigo Attack")]
    public class WendigoAttack : CardEffect
    {
        public int damageDone = 1;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            // If 'value' is '-1', then no value was passed and just use foodGained
            int damage = value != -1 ? value + damageDone : damageDone;
            var player = PlayerController.GetPlayer(target);
            Debug.Log($"Scavenge food by {damage}");
            
            if (player.IsSimulatingNight)
                player.hiddenHealth -= damage; // Night, so apply only for local player
            else
                player.health.Value -= damage; // Day, so apply for all players
        }
    }
}
