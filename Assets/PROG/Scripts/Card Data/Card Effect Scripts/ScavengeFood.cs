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
            var player = PlayerController.GetPlayer(target);
            Debug.Log($"Scavenge food by {food}");
            
            if (player.IsSimulatingNight)
                player.hiddenFood += food; // Night, so apply only for local player
            else
                player.food.Value += food; // Day, so apply for all players
        }
    }
}
