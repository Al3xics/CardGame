using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCPlayCardState : State<PlayerControllerSM>
	{
		public PCPlayCardState(PlayerControllerSM stateMachine) : base(stateMachine) { }

		public override void OnEnter()
		{
			base.OnEnter();
			PlayCard();
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

		public void PlayCard()
		{
			PlayerController.Instance.ConfirmPlay();

        }
    }
}
