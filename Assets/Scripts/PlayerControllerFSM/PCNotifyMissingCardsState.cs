using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCNotifyMissingCardsState : State<PlayerControllerSM>
	{
        private PlayerController _player;
        public PCNotifyMissingCardsState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

		public override void OnEnter()
		{
			base.OnEnter();
			_player.NotifyMissingCards();
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
