using UnityEngine;
using Wendogo;

public class PCTurnOverState : State<PlayerControllerSM>
{

	public PCTurnOverState(PlayerControllerSM stateMachine) : base(stateMachine) { }

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
