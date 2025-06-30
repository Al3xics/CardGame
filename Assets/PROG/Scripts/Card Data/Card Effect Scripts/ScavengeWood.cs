using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "ScavengeWood", menuName = "Card Effects/Scavenge Wood")]
    public class ScavengeWood : CardEffect
    {
        public int woodGained = 1;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            // If 'value' is '-1', then no value was passed and just use woodGained
            int wood = value != -1 ? value + woodGained : woodGained;
            var player = PlayerController.GetPlayer(target);
            Debug.Log($"Scavenge food by {wood}");
            
            if (player.IsSimulatingNight)
                player.hiddenFood += wood; // Night, so apply only for local player
            else
                player.food.Value += wood; // Day, so apply for all players
        }
    }
}
