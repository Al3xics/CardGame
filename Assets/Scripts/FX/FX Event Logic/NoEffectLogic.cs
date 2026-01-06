using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "NoEffectLogic", menuName = "FX/FX Event Logic/No Effect")]
    public class NoEffectLogic : FXEventLogicBase
    {
        public override void PreFX(FXEventAsset asset, FXEventContext context)
        {
        }

        public override void PostFX(FXEventAsset asset, FXEventContext context)
        {
        }
    }
}