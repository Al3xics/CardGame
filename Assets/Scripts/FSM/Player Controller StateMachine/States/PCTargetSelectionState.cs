using UnityEngine;

namespace Wendogo
{

    public class PCTargetSelectionState : State<PlayerControllerSM>
    {
        PlayerController _player;
        private bool _hasShownUI = false;
        public PCTargetSelectionState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override async void OnEnter()
        {
            base.OnEnter();

            CardDataSO cardToUse = _player.ActiveCard.Card;

            if (cardToUse.nightPriorityIndex > 0 && ServerManager.Instance.currentCycle.Value == Cycle.Night)
            {
                Debug.LogWarning("Carte de nuit ne peut pas être jouée pendant la nuit. Ignorée.");
                StateMachine.ChangeState<PCPlayCardState>();
                return;
            }

            Debug.Log($"active card is : {cardToUse.name}");
            _hasShownUI = true;
            _player._handManager.ToggleOffMovingCards(_player._handManager.handCards);
            cardToUse.CardEffect.ShowUI();

            if (cardToUse.isGroup)
            {
                await _player.GroupSelectTargetAsync();
                _player._handManager.ToggleOnMovingCards(_player._handManager.handCards);
                _player.EnableInput();
                ServerManager.Instance.ClearVoteRpc();
            }
            else if(cardToUse.CardEffect is BuildRitual)
                await _player.SelectRessourceAsync();
            else
                await _player.SelectTargetAsync();

            _player._handManager.ToggleOnMovingCards(_player._handManager.handCards);
            StateMachine.ChangeState<PCPlayCardState>();
        }

        public override void OnTick()
        {
            base.OnTick();
        }

        public override void OnExit()
        {
            if (_hasShownUI)
            {
                _hasShownUI = false;
                _player._handManager.ToggleOffMovingCards(_player._handManager.handCards);
                _player.ActiveCard.Card.CardEffect.HideUI(true);
            }
            base.OnExit();
        }
    }
}
