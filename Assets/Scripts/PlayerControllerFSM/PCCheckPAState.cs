using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PCCheckPAState : State<PlayerControllerSM>
    {
        public PCCheckPAState(PlayerControllerSM stateMachine) : base(stateMachine) { }

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
            if (PlayerController.Instance._playerPA > 0)
                StateMachine.ChangeState<PCInputState>();

            else
                StateMachine.ChangeState<PCTurnOverState>();
        }
    }
}
