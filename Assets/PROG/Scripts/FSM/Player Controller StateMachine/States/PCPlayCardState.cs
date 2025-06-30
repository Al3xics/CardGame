using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCPlayCardState : State<PlayerControllerSM>
	{
		PlayerController _player;
		public PCPlayCardState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

		public override void OnEnter()
		{
			base.OnEnter();
			_player.ConfirmPlay();
            _player._playerPA--;
            StateMachine.ChangeState<PCNotifyMissingCardsState>();
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
