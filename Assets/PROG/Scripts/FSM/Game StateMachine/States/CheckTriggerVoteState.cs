using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents a state in the GameStateMachine where the trigger for initiating a vote is checked.
    /// </summary>
    public class CheckTriggerVoteState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents a state within the game logic that checks whether certain conditions for triggering a vote have been met.
        /// </summary>
        public CheckTriggerVoteState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnCheckTriggerVote += OnCheckTriggerVote;
            CheckTriggerVote();
        }

        /// <summary>
        /// Checks the conditions to determine if a vote should be triggered within the game state logic.
        /// Transitions to the next appropriate game state based on the result of the check.
        /// </summary>
        private void CheckTriggerVote()
        {
            if (StateMachine.CheckVotingTurn())
                ServerManager.Instance.UseAllUIForVotersRpc(true, true);
            else
                OnCheckTriggerVote();
        }

        private void OnCheckTriggerVote()
        {
            ServerManager.Instance.OnCheckTriggerVote -= OnCheckTriggerVote;
            ServerManager.Instance.UseAllUIForVotersRpc(false, true);
            StateMachine.ChangeState<CheckLastTurnState>();
        }

        public override void OnExit()
        {
            base.OnExit();
            StateMachine.IncreaseCptTurnForVote();
        }
    }
}