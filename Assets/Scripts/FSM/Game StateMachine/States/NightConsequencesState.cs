using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents the state in the game where the consequences of the night phase are processed.
    /// </summary>
    public class NightConsequencesState : State<GameStateMachine>
    {
        private int id;
        private List<PlayerAction> sortedActions = new();
        private PlayerAction currentPlayerAction;
        
        /// <summary>
        /// Represents the state in which the consequences of actions taken during the night phase are processed.
        /// </summary>
        public NightConsequencesState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            
            ServerManager.Instance.MuteAllPlayersRpc(false);
            
            id = 0;
            // Sort `NightActions` by priority index and process them
            sortedActions = StateMachine.GetNightActionsWithPriority().OrderBy(card => card.CardPriorityIndex).ToList();
            
            ServerManager.Instance.SynchronizePlayerValuesRpc(false);
            StateMachine.CopyHiddenToPublic();
            
            if (sortedActions.Count != 0)
                ResolveCardNightConsequences();
            else
                NextState();
        }

        /// <summary>
        /// Resolves the consequences of a card during the night phase in the game.
        /// </summary>
        private void ResolveCardNightConsequences()
        {
            ServerManager.Instance.OnResolveCardNightConsequences += OnResolveCardNightConsequences;
            
            currentPlayerAction = sortedActions[id];
            currentPlayerAction.GetCardDataSO().CardEffect.ShowUI();
            ServerManager.Instance.EnableInputAndDisableMovingCardsRpc();
            ServerManager.Instance.GroupSelectTargetAsyncForAllPlayersRpc();
        }

        /// <summary>
        /// This method is triggered when the current card's effect completes its execution,
        /// progressing to the next card or transitioning to the next state if all cards have been processed.
        /// </summary>
        private void OnResolveCardNightConsequences()
        {
            ServerManager.Instance.OnResolveCardNightConsequences -= OnResolveCardNightConsequences;

            currentPlayerAction.GetCardDataSO().CardEffect.HideUI(false);
            Debug.Log("[NightConsequence] DisableInputAndMovingCardsRpc");
            
            currentPlayerAction.GetCardDataSO().CardEffect.Apply(0, 0);
            ServerManager.Instance.ClearVoteRpc();
            
            id++;
            bool isLast = id >= sortedActions.Count;

            if (isLast)
                NextState();
            else
                ResolveCardNightConsequences();
        }

        /// <summary>
        /// Transitions the game state from the current <see cref="NightConsequencesState"/> to the
        /// <see cref="CheckRitualState"/> after night consequences have been resolved.
        /// </summary>
        private void NextState()
        {
            ServerManager.Instance.CheckPlayerHealthRpc();
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}