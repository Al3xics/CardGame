using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendigoAttack", menuName = "Card Effects/Wendigo Attack")]
    public class WendigoAttack : CardEffect
    {
        public int damageDone = 1;
        private int temporaryTask = -1;
        
        public override async Task ApplyAsync(ulong owner, ulong target, int value = -1)
        {
            PlayerController player = PlayerController.GetPlayer(owner);
            await UniTask.WaitUntil(() => player.LaunchPlayerSelection(owner, value) >= (ulong)temporaryTask);
            
            Attack(owner, player.target, value);
        }

        public void Attack(ulong owner, ulong target, int value = -1)
        {
            PlayerController targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                targetPlayer.health.Value -= damageDone;
            }
        }
    }
}