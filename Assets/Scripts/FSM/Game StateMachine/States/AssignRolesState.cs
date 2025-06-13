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
            playerRoles[StateMachine.PlayersID[0]] = RoleType.Wendogo;

            for (int i = 1; i < StateMachine.PlayersID.Count; i++)
            {
                playerRoles[StateMachine.PlayersID[i]] = RoleType.Survivor;
            }

            Utils.DictionaryToArrays(playerRoles, out ulong[] roleTypeID, out RoleType[] roleType);
            ServerManager.Instance.AssignRolesToPlayersServerRpc(roleTypeID, roleType);
        }

        private void NextState()
        {
            ServerManager.Instance.OnAssignedRoles -= NextState;
            StateMachine.ChangeState<DistributeCardsState>();
        }
    }
}