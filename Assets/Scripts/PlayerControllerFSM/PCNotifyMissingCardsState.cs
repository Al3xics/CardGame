using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCNotifyMissingCardsState : State<PlayerControllerSM>
	{
		public PCNotifyMissingCardsState(PlayerControllerSM stateMachine) : base(stateMachine) { }

		public override void OnEnter()
		{
			base.OnEnter();
			//ServerManager.CallDelegate()
			StateMachine.ChangeState<PCCheckPAState>();
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
