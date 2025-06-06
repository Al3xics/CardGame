using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PCInputState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        public PCInputState(PlayerControllerSM stateMachine, PlayerController player ) : base(stateMachine) { _player = player; }

        public override void OnEnter()
        {
            base.OnEnter();
            CardClickHandler.OnCardClicked += ReceiveSelectedEvent; 
            _player.EnableInput();
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
            _player.SelectCard(cardObjectData);
            StateMachine.ChangeState<PCSelectionState>();
        }

    }
}
