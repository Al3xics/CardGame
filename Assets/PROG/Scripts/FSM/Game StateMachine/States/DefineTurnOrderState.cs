using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents the state where the turn order is defined for the game.
    /// This state performs the logic required to shuffle the player order
    /// and transitions to the <see cref="AssignRolesState"/>.
    /// </summary>
    public class DefineTurnOrderState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state within the game that defines the turn order for players.
        /// </summary>
        public DefineTurnOrderState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();

            if (AutoSessionBootstrapper.AutoConnect &&
                StateMachine.PlayersID.Count < AutoSessionBootstrapper.ExpectedPlayersCount)
            {
                throw new System.Exception("Not enough players to start the game.");
            }

            Shuffle(StateMachine.PlayersID);
            StateMachine.ChangeState<AssignRolesState>();
        }

        /// <summary>
        /// Randomly shuffles the elements of the given list in place.
        /// The order of elements in the list is randomized.
        /// </summary>
        /// <param name="list">The list of type <c>ulong</c> representing the player's unique IDs to be shuffled.</param>
        private void Shuffle(List<ulong> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randIndex = Random.Range(i, list.Count);
                (list[i], list[randIndex]) = (list[randIndex], list[i]);
            }
        }
    }
}