using System.Collections.Generic;
using System.Linq;

namespace Wendogo
{
    /// <summary>
    /// Represents the state in the game where the consequences of the night phase are processed.
    /// </summary>
    public class NightConsequencesState : State<GameStateMachine>
    {
        private int id;
        private List<PlayerAction> sortedActions = new();
        
        /// <summary>
        /// Represents the state in which the consequences of actions taken during the night phase are processed.
        /// </summary>
        public NightConsequencesState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            id = 0;
            // Sort `NightActions` by priority index and process them
            sortedActions = StateMachine.NightActions.Where(card => card.CardPriorityIndex > 0).OrderBy(card => card.CardPriorityIndex).ToList();
            
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
            ServerManager.Instance.UseAllUIForVotersRpc(true, true);
        }

        /// <summary>
        /// This method is triggered when the current card's effect completes its execution,
        /// progressing to the next card or transitioning to the next state if all cards have been processed.
        /// </summary>
        private void OnResolveCardNightConsequences()
        {
            ServerManager.Instance.OnResolveCardNightConsequences -= OnResolveCardNightConsequences;
            
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
            StateMachine.NightActions.Clear();
            ServerManager.Instance.CheckPlayerHealthRpc();
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}