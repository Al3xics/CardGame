using UnityEngine;
using Wendogo;

namespace Wendogo
{

	public class PCTargetSelectionState : State<PlayerControllerSM>
	{
        PlayerController _player;
        public PCTargetSelectionState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

		public override async void OnEnter()
		{
			base.OnEnter();
			PlayerUI.Instance.ToggleTargetSelectUI();
			//AwaitTarget();
			ulong selectedTarget = _player.GetChosenTarget();
			await _player.SelectTargetAsync(selectedTarget);
            StateMachine.ChangeState<PCPlayCardState>();
        }

		public override void OnTick()
		{
			base.OnTick();
		}

		public override void OnExit()
		{
			PlayerUI.Instance.ToggleTargetSelectUI();
			base.OnExit();
		}

		public void AwaitTarget()
		{
			_player.SelectTarget();

        }
	}
}
