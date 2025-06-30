using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PCNotifyMissingCardsState : State<PlayerControllerSM>
    {
        private PlayerController _player;
        public PCNotifyMissingCardsState(PlayerControllerSM stateMachine, PlayerController player) : base(stateMachine) { _player = player; }

        public async override void OnEnter()
        {
            base.OnEnter();
            ToggleMovingCards();
            int missingCards = _player.GetMissingCards();
            await _player.SelectDeckAsync(missingCards);
            StateMachine.ChangeState<PCCheckPAState>();
        }

        public override void OnTick()
        {
            base.OnTick();
        }

        public override void OnExit()
        {
            ToggleMovingCards();
            base.OnExit();
        }

        public void ToggleMovingCards()
        {
            System.Collections.Generic.List<GameObject> cardsInHand = _player._handManager._handCards;

            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = !handler.enabled;
            }
        }
    }
}