using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "TransitionToNightLogic", menuName = "FX/FX Event Logic/Transition To Night")]
    public class TransitionToNightLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> transitionToNightList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(transitionToNightList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}