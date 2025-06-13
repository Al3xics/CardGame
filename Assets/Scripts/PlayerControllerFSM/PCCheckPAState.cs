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
            if (_player.HasEnoughPA())
                StateMachine.ChangeState<PCInputState>();
            else
                StateMachine.ChangeState<PCTurnOverState>();
        }
    }
}
