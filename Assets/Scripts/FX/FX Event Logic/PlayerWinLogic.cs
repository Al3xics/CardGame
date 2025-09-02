using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "PlayerWinLogic", menuName = "FX/FX Event Logic/Player Win")]
    public class PlayerWinLogic : FXEventLogicBase
    {
        [SerializeField] private List<AudioClip> ritualCompleteList;
        
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            var randomElement = Utils.ChooseRandom(ritualCompleteList);
            asset.clip = randomElement;
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            ServerManager.Instance.IncrementEndGameAnimationFinishedCptRpc();
        }
    }
}