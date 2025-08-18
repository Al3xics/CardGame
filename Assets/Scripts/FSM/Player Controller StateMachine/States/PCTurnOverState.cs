using UnityEngine;

namespace Wendogo
{
    public class PCTurnOverState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        public PCTurnOverState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override void OnEnter()
        {
            StateMachine.playerPAUpdated -= _player.UpdatePA;
            Debug.Log("Enter end");
            base.OnEnter();
            if (_player._handManager._attackButton.activeSelf)
                _player._handManager._attackButton.SetActive(false);
            _player.HandleCancelTimer();
            PlayerUI.Instance.DefineTimer(60);
            _player.NotifyEndTurn();
        }

        public override void OnTick()
        {
            base.OnTick();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

    }
}