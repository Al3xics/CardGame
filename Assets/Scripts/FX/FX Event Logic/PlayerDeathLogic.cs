using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "PlayerDeathLogic", menuName = "FX/FX Event Logic/Player Death")]
    public class PlayerDeathLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> playerDeathList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(playerDeathList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            
        }
    }
}