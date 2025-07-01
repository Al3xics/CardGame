using UnityEngine;
using Wendogo;

namespace Wendogo
{

	public class PCTargetSelectionState : State<PlayerControllerSM>
	{
        PlayerController _player;
        public PCTargetSelectionState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

		public override void OnEnter()
		{
			base.OnEnter();
			_player.ToggleUI();
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
			_player.SelectTarget();
			StateMachine.ChangeState<PCPlayCardState>();
        }
	}
}
