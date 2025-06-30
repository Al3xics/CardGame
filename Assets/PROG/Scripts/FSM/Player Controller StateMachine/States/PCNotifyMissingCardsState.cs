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
            ToggleDeck();
            ToggleOffMovingCards(_player._handManager._handCards);
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
            ToggleDeck();
            ToggleOnMovingCards(_player._handManager._handCards);
            base.OnExit();
        }


        public void ToggleOffMovingCards(List<GameObject> cardsInHand)
        {
            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = false;
            }
        }
        public void ToggleOnMovingCards(List<GameObject> cardsInHand)
        {
            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = true;
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