namespace Wendogo
{
    /// <summary>
    /// Represents the state in the game where the consequences of the night phase are processed.
    /// </summary>
    public class NightConsequencesState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the state in which the consequences of actions taken during the night phase are processed.
        /// </summary>
        public NightConsequencesState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ResolveNightConsequences();
        }

        // todo
        private void ResolveNightConsequences()
        {
            // Tu parcours l’ordre de tour
            foreach (var playerId in StateMachine.PlayersID)
            {
                if (!StateMachine.NightActions.ContainsKey(playerId))
                    continue;

                foreach (var action in StateMachine.NightActions[playerId])
                {
                    // Exemples :
                    // - Récupérer les ressources de food AVANT
                    // - Appliquer dégâts
                    // - Voler ressources
                    // - Vérifier si la cible a une protection en attente

                    // Tu résous ici en fonction de la logique du jeu
                }
            }

            // Une fois résolu, tu peux notifier le serveur
            ServerManager.Instance.SendDataServerServerRpc(); // À adapter avec le résultat
            StateMachine.NightActions.Clear();
            NextState();
        }

        // todo
        private void CheckHealth()
        {
            foreach (var player in StateMachine.PlayersHealth)
            {
                // If dead
                if (player.Value == 0)
                {
                    // Remove player from the game (pass to viewer mode), annd from the players ID list
                    // Send to server manager to tell the player he is dead and need to change to viewer mode
                    // Inform all players that one player is dead
                }
            }

            // ServerManager.Instance.ChangePlayersHealth(StateMachine.PlayersHealth);
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