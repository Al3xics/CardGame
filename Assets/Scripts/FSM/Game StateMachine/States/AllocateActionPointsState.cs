using System.Collections.Generic;

namespace Wendogo
{
    public class AllocateActionPointsState : State<GameStateMachine>
    {
        public AllocateActionPointsState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnAllocatedActionPoint += NextState;
            AllocateActionPoints(StateMachine.pointPerTurn);
        }

        private void AllocateActionPoints(int allocatedPoints = 0)
        {
            Dictionary<ulong, int> playersActionPoints = new();

            foreach (var id in StateMachine.PlayersID)
            {
                playersActionPoints.Add(id, allocatedPoints);
            }
            
            //ServerManager.Instance.AllocateActionPointsToPlayers(playersActionPoints);
        }

        private void NextState()
        {
            ServerManager.Instance.OnAllocatedActionPoint -= NextState;
            StateMachine.ChangeState<PlayerTurnState>();
        }
    }
}