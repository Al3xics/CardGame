using UnityEngine;
using Wendogo;
using System.Collections.Generic;

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
            ToggleDeck();
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
            ToggleDeck();
            base.OnExit();
        }

        public void ToggleMovingCards()
        {
            List<GameObject> cardsInHand = _player._handManager._handCards;

            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = !handler.enabled;
            }
        }

        public void ToggleDeck()
        {
            DeckClickHandler[] deckClickHandlers = Object.FindObjectsByType<DeckClickHandler>(FindObjectsSortMode.None);
            foreach (DeckClickHandler deck in deckClickHandlers)
            {
                deck.enabled = !deck.enabled;
            }
        }
    }
}