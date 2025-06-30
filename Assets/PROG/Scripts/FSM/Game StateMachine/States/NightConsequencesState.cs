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
            
            ServerManager.Instance.SynchronizePlayerValuesServerRpc(false);
            ResolveCardNightConsequences(id);
        }

        // todo
        private void ResolveCardNightConsequences(int cpt)
        {
            ServerManager.Instance.OnResolveCardNightConsequences += OnResolveCardNightConsequences;
            
            var card = StateMachine.dataCollectionScript.cardDatabase.GetCardByID(sortedActions[cpt].CardId);
            card.CardEffect.Apply(sortedActions[cpt].OriginId, sortedActions[cpt].TargetId);
            // Here, only card that needs an action from the players will execute. When they finish,
            // 'OnResolvedCardNightConsequences' will be called.
        }

        // todo
        private void OnResolveCardNightConsequences()
        {
            ServerManager.Instance.OnResolveCardNightConsequences -= OnResolveCardNightConsequences;
            
            id++;
            bool isLast = id >= sortedActions.Count;
            
            if (isLast)
                NextState();
            else
                ResolveCardNightConsequences(id);
        }

        /// <summary>
        /// Transitions the game state from the current <see cref="NightConsequencesState"/> to the
        /// <see cref="CheckRitualState"/> after night consequences have been resolved.
        /// </summary>
        private void NextState()
        {
            StateMachine.NightActions.Clear();
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}