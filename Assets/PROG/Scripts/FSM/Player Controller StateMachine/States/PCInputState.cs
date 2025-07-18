using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PCInputState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        bool isTurnBeginning = false;
        public PCInputState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!isTurnBeginning)
            {
                _player._playerPA = 2;
                isTurnBeginning = true;
            }
            //if (ServerManager.Instance.CurrentCycle.Value == Cycle.Night && _player.Role.Value == RoleType.Wendogo)
            //{
            //    _player._handManager.DrawCard(_player.GetCardByID(10199));
            //}
            CardDropZone.OnCardDropped += ReceiveSelectedEvent;
            _player.EnableInput();
        }

        public override void OnTick()
        {
            base.OnTick();

        }

        public override void OnExit()
        {
            base.OnExit();
            CardDropZone.OnCardDropped -= ReceiveSelectedEvent;
        }

        public void ReceiveSelectedEvent(CardObjectData cardObjectData)
        {
            _player.SelectCard(cardObjectData);
            StateMachine.ChangeState<PCSelectionState>();
        }

    }
}
