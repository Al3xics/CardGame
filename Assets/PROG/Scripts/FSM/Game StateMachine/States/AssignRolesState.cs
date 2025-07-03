using System.Collections.Generic;

namespace Wendogo
{
    /// <summary>
    /// Represents the state in the game state machine responsible for assigning roles to players.
    /// </summary>
    /// <remarks>
    /// This state, upon entry, assigns roles to players and transitions to the next state once roles are assigned.
    /// It subscribes to the <c>OnAssignedRoles</c> event from the <c>ServerManager</c> to signal that the
    /// assignment process is complete.
    /// </remarks>
    public class AssignRolesState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state in the game where roles are assigned to players.
        /// </summary>
        public AssignRolesState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnAssignedRoles += NextState;
            AssignRoles();
        }

        /// <summary>
        /// Assigns specific roles to players within the game, based on their unique identifiers.
        /// </summary>
        /// <remarks>
        /// This method primarily assigns the "Wendogo" role to the first player in the player list
        /// and assigns the "Survivor" role to all remaining players. It then converts the assignments
        /// into arrays and invokes server-side functionality to distribute roles to players.
        /// </remarks>
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

        /// <summary>
        /// Proceeds to the next state in the game flow after roles have been assigned.
        /// This method is invoked when the role assignment process is completed.
        /// </summary>
        /// <remarks>
        /// Unsubscribes from the <see cref="ServerManager.OnAssignedRoles"/> event to
        /// prevent further invocation of this method, and transitions the state machine
        /// to the <see cref="DistributeCardsState"/>.
        /// </remarks>
        private void NextState()
        {
            ServerManager.Instance.OnAssignedRoles -= NextState;
            StateMachine.ChangeState<DistributeCardsState>();
        }
    }
}