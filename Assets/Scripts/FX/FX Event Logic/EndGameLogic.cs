using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "EndGameLogic", menuName = "FX/FX Event Logic/End Game")]
    public class EndGameLogic : FXEventLogicBase
    {
        public override void PreFX(FXEventContext context)
        {
            
        }

        public override void PostFX(FXEventContext context)
        {
            ServerManager.Instance.IncrementEndGameAnimationFinishedCptRpc();
        }
    }
}