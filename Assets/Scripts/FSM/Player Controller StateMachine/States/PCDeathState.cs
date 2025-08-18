namespace Wendogo
{
    public class PCDeathState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        
        public PCDeathState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override void OnEnter()
        {
            base.OnEnter();
            // Don't disable input, because there is still the option menu that can be clicked
            ServerManager.Instance.RemovePlayerFromListsRpc(_player.OwnerClientId);
            SessionManager.Instance.MutePlayer(true);
            _player._handManager.ToggleOffMovingCards(_player._handManager.handCards);
            ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
            {
                fxType = FXEventType.OnPlayerDeath,
                playerID = _player.OwnerClientId
            });
            
            
            
            /*
             * todo :
             * - il n'est plus compter dans les joueurs présent (donc on saute sont tour)
             * - il ne peut plus parler (mute mic)
             * - il ne peut plus interagir avec le jeux/cartes
             * - on enlève sont plateau de jeu et on lui met juste la scène avec un filtre qui indique sa mort
             * - il peut quand même voir les cartes qui sont jouer
             * - il peut pas quitter la partie (il doit attendre qu'elle soit fini)
             */
        }
    }
}