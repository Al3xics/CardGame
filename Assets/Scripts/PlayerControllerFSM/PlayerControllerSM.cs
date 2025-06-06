using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PlayerControllerSM : StateMachine<PlayerControllerSM>
    {
        private PlayerController _player;

        protected override void Start()
        {
            base.Start();
            _player = GetComponent<PlayerController>();
        }

        protected override State<PlayerControllerSM> GetInitialState()
        {
            PCInputState inputstate = new PCInputState(this);
            PCPlayCardState pCCardPlayState = new PCPlayCardState(this);
            PCCheckPAState pCCheckPAState = new PCCheckPAState(this);
            PCNotifyMissingCardsState pCNotifyMissingCardsState = new PCNotifyMissingCardsState(this);
            PCSelectionState pCSelectionState = new PCSelectionState(this);
            PCTargetSelectionState pCTargetSelectionState = new PCTargetSelectionState(this);
            PCTurnOverState pCTurnOverState = new PCTurnOverState(this);
            PCBurnCardState pCBurnCardState = new PCBurnCardState(this);

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

        private void Update()
        {
            CurrentState?.OnTick();
        }
    }
}