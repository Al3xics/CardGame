using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealWoodLogic", menuName = "FX/FX Event Logic/Steal Wood")]
    public class StealWoodLogic : FXEventLogicBase
    {
        [SerializeField] private AnimationClip stealWoodClipP2;
        [SerializeField] private AnimationClip stealWoodClipP3;
        [SerializeField] private AnimationClip stealWoodClipP4;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            if (context.fxType != FXEventType.OnStealWood)
                return;
            
            if (!PlayerController.PlayerSlots.TryGetValue(context.playerID, out int slotIndex))
                return;
            
            switch (slotIndex)
            {
                case 1: asset.animation = stealWoodClipP2; break;
                case 2: asset.animation = stealWoodClipP3; break;
                case 3: asset.animation = stealWoodClipP4; break;
                default: asset.animation = null; break;
            }
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}