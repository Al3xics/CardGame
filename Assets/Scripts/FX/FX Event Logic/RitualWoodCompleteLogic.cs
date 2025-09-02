using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "RitualWoodCompleteLogic", menuName = "FX/FX Event Logic/Ritual Wood Complete")]
    public class RitualWoodCompleteLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> ritualWoodCompleteList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(ritualWoodCompleteList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}