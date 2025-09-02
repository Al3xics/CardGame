using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "RitualFoodCompleteLogic", menuName = "FX/FX Event Logic/Ritual Food Complete")]
    public class RitualFoodCompleteLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> ritualFoodCompleteList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(ritualFoodCompleteList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}