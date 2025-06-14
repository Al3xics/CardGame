using UnityEngine;
using Wendogo;

public class PCTurnOverState : State<PlayerControllerSM>
{
	private PlayerController _player;
	public PCTurnOverState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { player = _player; }

	public override void OnEnter()
	{
		base.OnEnter();
		//ServerManager.Notify
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
