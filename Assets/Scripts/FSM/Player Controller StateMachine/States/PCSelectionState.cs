using UnityEngine;
using Wendogo;

namespace Wendogo
{
	public class PCSelectionState : State<PlayerControllerSM>
	{
		PlayerController _player;
		public PCSelectionState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

		public override void OnEnter()
		{
			base.OnEnter();
			//OnTargetDetected += PassToTarget;

            if (_player.ActiveCard.Card.HasTarget)
			{
				StateMachine.ChangeState<PCTargetSelectionState>();
			}
			//else if (_player.ActiveCard.Card.Cost > 0)
			//{
			//	CheckRessources();
			//}
			else 
			{
                StateMachine.ChangeState<PCPlayCardState>();
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

		public void PassToTarget()
		{
            StateMachine.ChangeState<PCTargetSelectionState>();
        }

		public void CheckRessources()
		{
			//	If (Playercontroller.ressources >= requiredCardRessources)
			//	{ StateMachine.ChangeState<PCCardPlayState>}
			//	else
			//	{
			//		_player.DeselectCard(PlayerController.Instance.ActiveCard);
			//              _player.ActiveCard = null;
			//		StateMachine.ChangeState<PCInputState>();
			//	}
		}
    }
}
