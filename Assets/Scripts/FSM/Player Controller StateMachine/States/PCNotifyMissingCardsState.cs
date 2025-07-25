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
            HandManager handManager = _player._handManager;
            if(handManager.handCards.Count == handManager._maxHandSize)
            {
                StateMachine.ChangeState<PCCheckPAState>();
                Debug.Log("str");
            }
            ToggleDeck();
            handManager.ToggleOffMovingCards(handManager.handCards);
            int missingCards = _player.GetMissingCards();
            
            await _player.SelectDeckAsync(missingCards);
            
            ToggleDeck();
            _player._handManager.ToggleOnMovingCards(_player._handManager.handCards);
            StateMachine.ChangeState<PCCheckPAState>();
        }

        public override void OnTick()
        {
            base.OnTick();
        }

        public override void OnExit()
        {
            base.OnExit();
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