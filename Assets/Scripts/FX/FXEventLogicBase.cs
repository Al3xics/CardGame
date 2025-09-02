using UnityEngine;

namespace Wendogo
{
    public abstract class FXEventLogicBase : ScriptableObject
    {
        public abstract void PreFX(FXEventAsset asset, FXEventContext context);
        public abstract void PostFX(FXEventAsset asset, FXEventContext context);
    }
}