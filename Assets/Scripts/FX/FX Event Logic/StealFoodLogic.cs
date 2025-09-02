using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealFoodLogic", menuName = "FX/FX Event Logic/Steal Food")]
    public class StealFoodLogic : FXEventLogicBase
    {
        [SerializeField] private AnimationClip stealFoodClipP2;
        [SerializeField] private AnimationClip stealFoodClipP3;
        [SerializeField] private AnimationClip stealFoodClipP4;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            if (context.fxType != FXEventType.OnStealFood)
                return;

            if (!PlayerController.PlayerSlots.TryGetValue(context.playerID, out int slotIndex))
                return;
            
            switch (slotIndex)
            {
                case 1: asset.animation = stealFoodClipP2; break;
                case 2: asset.animation = stealFoodClipP3; break;
                case 3: asset.animation = stealFoodClipP4; break;
                default: asset.animation = null; break;
            }
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}