using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PCCheckPAState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        public PCCheckPAState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override void OnEnter()
        {
            base.OnEnter();
            CheckPA();
        }

        public override void OnTick()
        {
            base.OnTick();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public void CheckPA()
        {
            Debug.Log($"Player PA is {_player._playerPA}");

            if (_player.HasEnoughPA())
            {
                StateMachine.ChangeState<PCInputState>();
                Debug.Log("has enough PA");
            }
            else
            {
                Debug.Log("has not enough PA");
                StateMachine.ChangeState<PCTurnOverState>();
            }
        }
    }
}
