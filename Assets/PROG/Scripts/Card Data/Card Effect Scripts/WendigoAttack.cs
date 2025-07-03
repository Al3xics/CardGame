using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendigoAttack", menuName = "Card Effects/Wendigo Attack")]
    public class WendigoAttack : CardEffect
    {
        public int damageDone = 1;
        //public int temporaryTask = -1;
        
        //public override event Action OnTargetDetection;

        
        public override async Task ApplyAsync(ulong owner, ulong target, int value = -1)
        {
            //OnTargetDetection?.Invoke();
            PlayerController player = PlayerController.GetPlayer(owner);
            await player.LaunchPlayerSelection(owner, value);
            //await UniTask.WaitUntil(() => player.LaunchPlayerSelection(owner, value) >= (ulong)temporaryTask);
            
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