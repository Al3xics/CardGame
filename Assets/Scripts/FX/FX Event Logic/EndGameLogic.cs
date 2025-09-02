using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "EndGameLogic", menuName = "FX/FX Event Logic/End Game")]
    public class EndGameLogic : FXEventLogicBase
    {
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
            
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
            ServerManager.Instance.IncrementEndGameAnimationFinishedCptRpc();
        }
    }
}