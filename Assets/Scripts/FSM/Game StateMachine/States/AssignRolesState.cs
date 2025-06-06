using System.Collections.Generic;

namespace Wendogo
{
    public class AssignRolesState : State<GameStateMachine>
    {
        public AssignRolesState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnAssignedRoles += NextState;
            AssignRoles();
        }

        private void AssignRoles()
        {
            Dictionary<ulong, RoleType> playerRoles = new();
            playerRoles[GameStateMachine.Instance.PlayersID[0]] = RoleType.Wendogo;

            for (int i = 1; i < GameStateMachine.Instance.PlayersID.Count; i++)
            {
                playerRoles[GameStateMachine.Instance.PlayersID[i]] = RoleType.Survivor;
            }
            
            ServerManager.Instance.AssignRolesToPlayers(playerRoles);
        }

        private void NextState()
        {
            ServerManager.Instance.OnAssignedRoles -= NextState;
            StateMachine.ChangeState<DistributeCardsState>();
        }
    }
}