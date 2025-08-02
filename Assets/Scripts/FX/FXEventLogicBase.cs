using UnityEngine;

namespace Wendogo
{
    public abstract class FXEventLogicBase : ScriptableObject
    {
        public abstract void PreFX(FXEventContext context);
        public abstract void PostFX(FXEventContext context);
    }
}