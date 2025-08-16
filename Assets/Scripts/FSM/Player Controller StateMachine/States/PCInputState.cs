using System;
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
                StateMachine.playerPAUpdated += _player.UpdatePA;

                _player._playerPA = 2;
                StateMachine.RaisePlayerPAUpdated(_player._playerPA); 
                isTurnBeginning = true;
            }
            if (_player.Role.Value == RoleType.Wendogo)
            {
                _player._handManager._leurreButton.SetActive(true);
                if(ServerManager.Instance.currentCycle.Value == Cycle.Night)
                    _player._handManager._attackButton.SetActive(true);
            }
            CardDropZone.OnCardDropped += ReceiveSelectedEvent;
            _player.EnableInput();
            _player._handManager.ToggleOnMovingCards(_player._handManager.handCards);
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
