using UnityEngine;
using Wendogo;

namespace Wendogo
{
    //The state machine for the player controller based on the template
    //Used to insure clean state transition for the players
    public class PlayerControllerSM : StateMachine<PlayerControllerSM>
    {
        [SerializeField] private PlayerController _player;

        protected override void Start()
        {
            base.Start();
            //_player = GetComponent<PlayerController>();
        }

        protected override State<PlayerControllerSM> GetInitialState()
        {
            PCInputState inputstate = new PCInputState(this, _player);
            PCPlayCardState pCCardPlayState = new PCPlayCardState(this, _player);
            PCCheckPAState pCCheckPAState = new PCCheckPAState(this, _player);
            PCNotifyMissingCardsState pCNotifyMissingCardsState = new PCNotifyMissingCardsState(this, _player);
            PCSelectionState pCSelectionState = new PCSelectionState(this, _player);
            PCTargetSelectionState pCTargetSelectionState = new PCTargetSelectionState(this, _player);
            PCTurnOverState pCTurnOverState = new PCTurnOverState(this, _player);
            PCBurnCardState pCBurnCardState = new PCBurnCardState(this, _player);

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