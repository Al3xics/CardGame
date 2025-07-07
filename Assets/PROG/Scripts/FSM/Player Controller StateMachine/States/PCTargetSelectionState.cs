using UnityEngine;
using Wendogo;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.UI.Image;

namespace Wendogo
{

    public class PCTargetSelectionState : State<PlayerControllerSM>
    {
        PlayerController _player;
        public PCTargetSelectionState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public override async void OnEnter()
        {
            base.OnEnter();
            //PlayerUI.Instance.ToggleTargetSelectUI();
            //AwaitTarget();
            _player.ActiveCard.Card.CardEffect.ShowUI();
            Debug.Log($"active card is : {_player.ActiveCard.Card.name}");
            _player._handManager.ToggleOffMovingCards(_player._handManager.handCards);

            if (_player.ActiveCard.Card.isGroup)
            {
                ulong selectedTarget = _player.GetChosenTarget();
                await _player.GroupSelectTargetAsync(selectedTarget);
            }
            else
            {
                ulong selectedTarget = _player.GetChosenTarget();
                await _player.SelectTargetAsync(selectedTarget);
            }
            StateMachine.ChangeState<PCPlayCardState>();
        }

        public override void OnTick()
        {
            base.OnTick();
        }

        public override void OnExit()
        {
            _player._handManager.ToggleOffMovingCards(_player._handManager.handCards);
            _player.ActiveCard.Card.CardEffect.HideUI();
            base.OnExit();
        }

        public void AwaitTarget()
        {
            _player.SelectTarget();

        }
    }
}
