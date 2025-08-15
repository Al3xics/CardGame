using System;

namespace Wendogo
{
    public static class GameEvents
    {
        public static event Action<FXEventContext> OnFXEvent;
        
        public static void RaiseLocalFX(FXEventContext fxEventContext) => OnFXEvent?.Invoke(fxEventContext);
    }
}