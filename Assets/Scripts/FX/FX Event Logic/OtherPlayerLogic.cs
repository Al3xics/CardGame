using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "OtherPlayerLogic", menuName = "FX/FX Event Logic/Other Player")]
    public class OtherPlayerLogic : FXEventLogicBase
    {
        [SerializeField] private AnimationClip deathClipP2;
        [SerializeField] private AnimationClip deathClipP3;
        [SerializeField] private AnimationClip deathClipP4;
        [SerializeField] private List<AudioClip> playerDeathList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            if (context.fxType != FXEventType.OnOtherPlayerDeath)
                return;
            
            if (!PlayerController.PlayerSlots.TryGetValue(context.playerID, out int slotIndex))
                return;
            
            
            // Animation
            switch (slotIndex)
            {
                case 1: asset.animation = deathClipP2; break;
                case 2: asset.animation = deathClipP3; break;
                case 3: asset.animation = deathClipP4; break;
                default: asset.animation = null; break;
            }
            
            
            // Sound
            var randomElement = Utils.ChooseRandom(playerDeathList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}