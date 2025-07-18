namespace Wendogo
{
    /// <summary>
    /// Represents the final state in the game where all gameplay processes have concluded.
    /// </summary>
    public class EndGameState : State<GameStateMachine>
    {
        /// <summary>
        /// Represents the end-game state within the game's state machine.
        /// This state signifies the completion of a game session and is used to
        /// handle any logic required at the conclusion of the game lifecycle.
        /// </summary>
        public EndGameState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ServerManager.Instance.OnAnimationFinished += OnAnimationsFinished;
            CheckWhoWon();
        }

        private void CheckWhoWon()
        {
            // todo
            // 
            // lancer l'animation en fonction de qui a gagné
            // 
            // si 'StateMachine.IsRitualOver' est true, c'est que les 'Survivor' on gagné --> afficher l'UI de WIN pour tous ceux là
            //          et donc le 'Wendogo' a perdu --> afficher l'UI LOST seulement pour lui
            // sinon, c'est que c'est le 'Wendigo' a gagné --> afficher l'UI WIN seulement pour lui
            //          et donc les 'Survivor' ont perdu --> afficher l'UI LOST pour tous ceux là
            
            
            // todo --> ou est-ce qu'on stocke l'UI de WIN et LOSE pour le Wendogo et les Players ????????
        }

        private void OnAnimationsFinished()
        {
            ServerManager.Instance.OnAnimationFinished -= OnAnimationsFinished;
            
            // todo
            //
            // changer de scène ici et retourner au menu pour tout le monde
        }
    }
}