using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    public class DefineTurnOrderState : State<GameStateMachine>
    {
        public DefineTurnOrderState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();

            Shuffle(StateMachine.PlayersID);
            StateMachine.ChangeState<AssignRolesState>();
        }

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