using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "WendogoWinLogic", menuName = "FX/FX Event Logic/Wendogo Win")]
    public class WendogoWinLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> wendogoWinList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(wendogoWinList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            ServerManager.Instance.IncrementEndGameAnimationFinishedCptRpc();
        }
    }
}