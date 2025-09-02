using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "RitualFakeCompleteLogic", menuName = "FX/FX Event Logic/Ritual Fake Complete")]
    public class RitualFakeFoodCompleteLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> ritualFakeCompleteList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(ritualFakeCompleteList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}