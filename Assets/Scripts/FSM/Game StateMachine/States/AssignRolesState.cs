using System.Collections.Generic;
using UnityEngine;

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
        /// This method assigns the "Wendogo" role to a specific player if defined by wendogoId,
        /// or randomly assigns it if wendogoId is -1. All remaining players are assigned the "Survivor" role.
        /// </remarks>
        private void AssignRoles()
        {
            Dictionary<ulong, RoleType> playerRoles = new();

            // Retrieve custom Wendogo ID from Game Settings (or fall back to random)
            ulong? selectedWendogoID = StateMachine.wendogoId != -1 ? (ulong)StateMachine.wendogoId : null;
            
            // Validate if the custom Wendogo ID exists in the current player list
            if (selectedWendogoID.HasValue && !StateMachine.PlayersID.Contains(selectedWendogoID.Value))
            {
                Debug.LogWarning($"Provided Wendogo ID {selectedWendogoID.Value} is invalid. Falling back to random assignment.");
                selectedWendogoID = null; // Reset to fall back to random
            }

            // Select Wendogo ID
            ulong wendogoID = selectedWendogoID ?? StateMachine.PlayersID[Random.Range(0, StateMachine.PlayersID.Count)];

            // Assign Wendogo
            playerRoles[wendogoID] = RoleType.Wendogo;

            // Assign Survivors to other players
            foreach (ulong playerId in StateMachine.PlayersID)
            {
                if (playerId != wendogoID)
                {
                    playerRoles[playerId] = RoleType.Survivor;
                }
            }

            // Convert dictionary to server arrays
            Utils.DictionaryToArrays(playerRoles, out ulong[] roleTypeID, out RoleType[] roleType);
            ServerManager.Instance.AssignRolesToPlayersRpc(roleTypeID, roleType);
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