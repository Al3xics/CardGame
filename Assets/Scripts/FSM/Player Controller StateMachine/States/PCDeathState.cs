using UnityEngine;

namespace Wendogo
{
    public class PCDeathState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        
        public PCDeathState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override void OnEnter()
        {
            base.OnEnter();
            _player.NotifyDeathRpc();
            ServerManager.Instance.RemovePlayerFromListsRpc(_player.OwnerClientId);
            SessionManager.Instance.MutePlayer(true);
            _player._handManager.ToggleOffMovingCards(_player._handManager.handCards);
            ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
            {
                fxType = FXEventType.OnPlayerDeath,
                playerID = _player.OwnerClientId
            });
            AwaitDisableCanvas();
        }

        private async void AwaitDisableCanvas()
        {
            await DeathUIManager.Instance.WaitUntilAllDestroyed();
            if (_player.isPlayerTurn)
            {
                _player.NotifyEndTurn();
                _player.isPlayerTurn = false;
            }
            Debug.Log("======================= Player Death =======================");
        }
    }
}