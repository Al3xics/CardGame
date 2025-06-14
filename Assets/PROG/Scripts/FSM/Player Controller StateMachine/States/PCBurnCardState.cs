using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCBurnCardState : State<PlayerControllerSM>
	{
        private PlayerController _player;

        public PCBurnCardState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

		public override void OnEnter()
		{
			base.OnEnter();
			_player.BurnCard();
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
