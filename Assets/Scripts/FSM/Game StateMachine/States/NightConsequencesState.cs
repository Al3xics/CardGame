namespace Wendogo
{
    public class NightConsequencesState : State<GameStateMachine>
    {
        public NightConsequencesState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnNightConsequencesEnded += NextState;
            ResolveNightConsequences();
        }

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
                    // - Appliquer dégâts
                    // - Voler ressources
                    // - Vérifier si la cible a une protection en attente

                    // Tu résous ici en fonction de la logique du jeu
                }
            }

            // Une fois résolu, tu peux notifier le serveur
            ServerManager.Instance.SendDataServerServerRpc(); // À adapter avec le résultat
            StateMachine.NightActions.Clear();
        }

        private void NextState()
        {
            ServerManager.Instance.OnNightConsequencesEnded -= NextState;
            StateMachine.NightActions.Clear();
            StateMachine.ChangeState<CheckRitualState>();
        }
    }
}