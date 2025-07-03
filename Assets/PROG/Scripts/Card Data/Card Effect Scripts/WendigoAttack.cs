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

        [HideInInspector]
        public GameObject prefabUI;

        private void Awake()
        {
            prefabUI = FindAnyObjectByType<TargetSelectionUI>(FindObjectsInactive.Include).gameObject;
        }

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            

            var targetPlayer = PlayerController.GetPlayer(target);
            if (targetPlayer != null)
            {
                targetPlayer.hiddenHealth -= damageDone;
            }
        }

        public override void ShowUI()
        {
            prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(true);
        }
        
        public override void HideUI()
        {
            prefabUI.SetActive(false);
        }
    }
}