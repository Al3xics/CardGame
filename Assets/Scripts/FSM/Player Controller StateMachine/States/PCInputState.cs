using System;
using UnityEngine;
using UnityEngine.UIElements;
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

                TimerStart();

                StateMachine.RaisePlayerPAUpdated(_player._playerPA);
                _player._handManager.trashZone.enabled = true;
                isTurnBeginning = true;
            }
            if (_player.Role.Value == RoleType.Wendogo)
            {
                _player._handManager._leurreButton.SetActive(true);
                _player._handManager._attackButton.SetActive(true);
            }
            CardDropZone.OnCardDropped += ReceiveSelectedEvent;
            CardDropZone.OnCardBurned += ReceiveBurningEvent;
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
            CardDropZone.OnCardBurned -= ReceiveBurningEvent;
        }

        public void ReceiveSelectedEvent(CardObjectData cardObjectData)
        {
            _player.SelectCard(cardObjectData);
            StateMachine.ChangeState<PCSelectionState>();
        }

        public void ReceiveBurningEvent(CardObjectData cardObjectData)
        {
            _player.SelectCard(cardObjectData);
            StateMachine.ChangeState<PCBurnCardState>();
        }

        public void TimerStart()
        {
            _player.eventTimer.StartTimer(60);
        }

    }
}
