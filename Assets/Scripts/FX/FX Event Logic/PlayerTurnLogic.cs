using TMPro;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "PlayerTurnLogic", menuName = "FX/FX Event Logic/Player Turn")]
    public class PlayerTurnLogic : FXEventLogicBase
    {
        public override void PreFX(FXEventContext context)
        {
            var popupText = FXEventManager.Instance.popupText;
            
            if (context.Player == PlayerController.LocalPlayer)
                popupText.text = PopupSentences.Instance.thisPlayerTurnText;
            else
            {
                string playerName = AutoSessionBootstrapper.AutoConnect
                    ? context.Player.name
                    : ServerManager.Instance.GetPlayerName(context.Player.OwnerClientId);
                popupText.text = PopupSentences.Instance.ReplaceX(PopupSentences.Instance.otherPlayerTurnText, playerName);
            }
        }

        public override void PostFX(FXEventContext context)
        {
            FXEventManager.Instance.popupText.text = "";
        }
    }
}