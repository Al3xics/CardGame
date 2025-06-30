using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "PointAction", menuName = "Card Effects/Point Action")]
    public class PointAction : CardEffect
    {
        public int points = 2;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var player = PlayerController.GetPlayer(owner);
            player._playerPA += points;
        }
    }
}
