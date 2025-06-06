using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Wendogo
{
    public class PlayerController : SerializedMonoBehaviour
    {
        [HideInInspector] public CardObjectData ActiveCard;
        [SerializeField] public EventSystem _inputEvent;
        [SerializeField] private HandManager _handManager;

        Dictionary<Button, PlayerController> playerTargets = new Dictionary<Button, PlayerController>();

        List<ulong> playerList = new List<ulong>();

        public int _playerPA;
        public int deckID;

        public static event Action OnCardUsed;

        public bool TargetSelected;


        private ulong _selectedTarget;

        private void Start()
        {
            _playerPA = 2;
        }

        public void EnableInput()
        {
            _inputEvent.enabled = true;

            //Implement enable input
            Debug.Log("Input enabled");
        }

        public async void SelectCard(CardObjectData card)
        {
            //Implement select card

            if (ActiveCard != null)
                DeselectCard(ActiveCard);

            ActiveCard = card;

            TweeningManager.CardUp(card.gameObject.transform);
            card.isSelected = true;

            if (ActiveCard.Card.HasTarget)
                SelectTarget();

            Debug.Log("Card selected");
        }

        public void DeselectCard(CardObjectData card)
        {
            //Implement deselect card
            Debug.Log("Card deselected");

            TweeningManager.CardDown(card.gameObject.transform);
            card.isSelected = false;
        }

        public async void SelectTarget()
        {
            //Implement select target
            TargetSelected = false;
            //Target selection
            //Handled the cancel next
            await UniTask.WaitUntil(() => TargetSelected);
            Debug.Log("Target selected");

            ConfirmPlay();
        }


        public void BurnCard()
        {
            //Implement burn card
            if (ActiveCard == null)
                return;

            Debug.Log("Card burnt");

            HandleUsedCard();
            //Placeholder for sending card lacking to server        
            NotifyMissingCards();
            CheckPA();
        }

        public void ConfirmPlay()
        {
            //Implement confirm play
            if (ActiveCard == null)
                return;

            //checkCardsconditions
            //if conditions aren't met: return;

            Debug.Log("Card played");

            //Apply effect
            //Not needed because handled by server?
            NotifyPlayedCard();

            HandleUsedCard();
            CheckPA();

        }

        public void HandleUsedCard()
        {
            //Use _playerPA
            _playerPA--;

            //Remove the card from the hand
            _handManager.Discard(ActiveCard.gameObject);

            if (ActiveCard.Card.isPassive)
            {
                Debug.Log("Passive card placed");
            }

            //Destroy(ActiveCard.gameObject);

        }

        public void CheckPA()
        {
            //Check player PA
            if (_playerPA > 0)
                return;

            else
            {
                //Placeholder for sending card lacking to server        
                NotifyMissingCards();
                Debug.Log("Turn is over");
                //Send informations to server
                NotifyEndTurn();
                //Send IDs of played cards to server
                Debug.Log("Sending to server");
                _inputEvent.enabled = false;
            }
        }

        public bool HasEnoughPA()
        {
            return _playerPA >= 0;
        }

        public ulong GetChosenTarget()
        {
            return _selectedTarget;
        }

        public void NotifyPlayedCard()
        {
            //ServerManager.Instance.TransmitPlayedCard(ActiveCard.Card.ID, _selectedTarget);
        }

        public int GetMissingCards()
        {
            return _handManager._maxHandSize - _handManager._handCards.Count;
        }

        public void NotifyMissingCards()
        {

            //ServerManager.Instance.TransmitMissingCards(GetMissingCards(), deckID)
        }

        public async void NotifyEndTurn()
        {
            //ServerManager.Instance.TransmitFinishedTurn();
        }

        //Create card with owner directly here
    }
}