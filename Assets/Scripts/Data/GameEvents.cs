using System;

namespace Wendogo
{
    public static class GameEvents
    {
        #region Actions

        public static event Action<FXEventContext> OnPlayerTurn;
        public static event Action<FXEventContext> OnPlayerWin;
        public static event Action<FXEventContext> OnPlayerLose;
        public static event Action<FXEventContext> OnWendogoWin;
        public static event Action<FXEventContext> OnWendogoLose;

        #endregion

        #region Raise Events
        
        public static void RaiseLocalFX(FXEventContext fxEventContext)
        {
            switch (fxEventContext.fxType)
            {
                case FXEventType.None:
                    break;
                case FXEventType.OnPlayerTurn:
                    OnPlayerTurn?.Invoke(fxEventContext);
                    break;
                case FXEventType.OnPlayerWin:
                    OnPlayerWin?.Invoke(fxEventContext);
                    break;
                case FXEventType.OnPlayerLose:
                    OnPlayerLose?.Invoke(fxEventContext);
                    break;
                case FXEventType.OnWendogoWin:
                    OnWendogoWin?.Invoke(fxEventContext);
                    break;
                case FXEventType.OnWendogoLose:
                    OnWendogoLose?.Invoke(fxEventContext);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(fxEventContext.fxType), fxEventContext.fxType, null);
            }
        }

        #endregion
    }
}