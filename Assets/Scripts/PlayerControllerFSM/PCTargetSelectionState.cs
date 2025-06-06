using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCTargetSelectionState : State<PlayerControllerSM>
	{
		public PCTargetSelectionState(PlayerControllerSM stateMachine) : base(stateMachine) { }

		public override void OnEnter()
		{
			base.OnEnter();
			AwaitTarget();
		}

		public override void OnTick()
		{
			base.OnTick();
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public void AwaitTarget()
		{
			PlayerController.Instance.SelectTarget();
			StateMachine.ChangeState<PCPlayCardState>();

        }
	}
}
