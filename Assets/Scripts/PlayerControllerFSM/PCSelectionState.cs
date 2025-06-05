using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCSelectionState : State<PlayerControllerSM>
	{
		public PCSelectionState(PlayerControllerSM stateMachine) : base(stateMachine) { }

		public override void OnEnter()
		{
			base.OnEnter();
			if(PlayerController.Instance.ActiveCard.Card.HasTarget)
			{
				StateMachine.ChangeState<PCTargetSelectionState>();
			}
			else if(PlayerController.Instance.ActiveCard.Card.Cost >0)
			{
				CheckRessources();
			}
		}

		public override void OnTick()
		{
			base.OnTick();
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public void CheckRessources()
		{
            //If Playercontroller.ressources >= requiredCardRessources
            //{ StateMachine.ChangeState<PCCardPlayState>}
            //else
            //{
            //PlayerController.DeselectCard(PlayerController.Instance.ActiveCard);
            //PlayerController.Instance.ActiveCard = null;
            //StateMachine.ChangeState<PCInputState>
            //}
        }

    }
}
