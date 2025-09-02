using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitualWoodLogic", menuName = "FX/FX Event Logic/Build Ritual Wood")]
    public class BuildRitualWoodLogic : FXEventLogicBase
    {
        [SerializeField] private AnimationClip buildRitualWoodClipP1;
        [SerializeField] private AnimationClip buildRitualWoodClipP2;
        [SerializeField] private AnimationClip buildRitualWoodClipP3;
        [SerializeField] private AnimationClip buildRitualWoodClipP4;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            if (context.fxType != FXEventType.OnBuildRitualWood)
                return;
            
            if (!PlayerController.PlayerSlots.TryGetValue(context.playerID, out int slotIndex))
            {
                asset.animation = buildRitualWoodClipP1;
                return;
            }
            
            switch (slotIndex)
            {
                case 1: asset.animation = buildRitualWoodClipP2; break;
                case 2: asset.animation = buildRitualWoodClipP3; break;
                case 3: asset.animation = buildRitualWoodClipP4; break;
                default: asset.animation = null; break;
            }
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}