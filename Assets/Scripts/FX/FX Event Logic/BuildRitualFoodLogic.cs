using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitualFoodLogic", menuName = "FX/FX Event Logic/Build Ritual Food")]
    public class BuildRitualFoodLogic : FXEventLogicBase
    {
        [SerializeField] private AnimationClip buildRitualFoodClipP1;
        [SerializeField] private AnimationClip buildRitualFoodClipP2;
        [SerializeField] private AnimationClip buildRitualFoodClipP3;
        [SerializeField] private AnimationClip buildRitualFoodClipP4;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            if (context.fxType != FXEventType.OnBuildRitualFood)
                return;
            
            if (!PlayerController.PlayerSlots.TryGetValue(context.playerID, out int slotIndex))
            {
                asset.animation = buildRitualFoodClipP1;
                return;
            }
            
            switch (slotIndex)
            {
                case 1: asset.animation = buildRitualFoodClipP2; break;
                case 2: asset.animation = buildRitualFoodClipP3; break;
                case 3: asset.animation = buildRitualFoodClipP4; break;
                default: asset.animation = null; break;
            }
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}