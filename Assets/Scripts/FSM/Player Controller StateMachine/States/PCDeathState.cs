namespace Wendogo
{
    public class PCDeathState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        
        public PCDeathState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }
        
        
    }
}