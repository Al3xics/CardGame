using Unity.Services.Matchmaker.Models;
using UnityEngine;
using Wendogo;

namespace Wendogo
{
    //The state machine for the player controller based on the template
    //Used to insure clean state transition for the players
    public class PlayerControllerSM : StateMachine<PlayerControllerSM>
    {
        protected override State<PlayerControllerSM> GetInitialState()
        {
            PCInputState inputstate = new PCInputState(this, PlayerController.LocalPlayer);
            PCPlayCardState pCCardPlayState = new PCPlayCardState(this, PlayerController.LocalPlayer);
            PCCheckPAState pCCheckPAState = new PCCheckPAState(this, PlayerController.LocalPlayer);
            PCNotifyMissingCardsState pCNotifyMissingCardsState = new PCNotifyMissingCardsState(this, PlayerController.LocalPlayer);
            PCSelectionState pCSelectionState = new PCSelectionState(this, PlayerController.LocalPlayer);
            PCTargetSelectionState pCTargetSelectionState = new PCTargetSelectionState(this, PlayerController.LocalPlayer);
            PCTurnOverState pCTurnOverState = new PCTurnOverState(this, PlayerController.LocalPlayer);
            PCBurnCardState pCBurnCardState = new PCBurnCardState(this, PlayerController.LocalPlayer);

            AddState(inputstate);
            AddState(pCCardPlayState);
            AddState(pCCheckPAState);
            AddState(pCNotifyMissingCardsState);
            AddState(pCSelectionState);
            AddState(pCTargetSelectionState);
            AddState(pCTurnOverState);
            AddState(pCBurnCardState);

            return inputstate;

        }
    }
}