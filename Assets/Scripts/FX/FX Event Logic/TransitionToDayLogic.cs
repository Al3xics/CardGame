using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "TransitionToDayLogic", menuName = "FX/FX Event Logic/Transition To Day")]
    public class TransitionToDayLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> transitionToDayList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(transitionToDayList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}