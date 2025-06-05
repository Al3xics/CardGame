using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PCInputState : State<PlayerControllerSM>
    {
        public PCInputState(PlayerControllerSM stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CardClickHandler.OnCardClicked += ReceiveSelectedEvent; 
            PlayerController.Instance.EnableInput();
        }

        public override void OnTick()
        {
            base.OnTick();
            
        }

        public override void OnExit()
        {
            base.OnExit();
            CardClickHandler.OnCardClicked -= ReceiveSelectedEvent;
        }

        public void ReceiveSelectedEvent(CardObjectData cardObjectData)
        {
            PlayerController.Instance.SelectCard(cardObjectData);
            StateMachine.ChangeState<PCSelectionState>();
        }

    }
}
