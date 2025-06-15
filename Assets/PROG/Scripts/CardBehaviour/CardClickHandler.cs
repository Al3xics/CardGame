using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wendogo
{
    //Handles player interactions with individual cards via touch
    public class CardClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private CardObjectData _cardObjectData; //Reference to the CardObjectData on this GameObject

        public static event Action<CardObjectData> OnCardClicked; //Event when any card is clicked

        public PlayerController Owner { get; set; } //Define card ownership

        private void Awake()
        {
            //Get the CardObjectData component attached to this GameObject
            _cardObjectData = GetComponent<CardObjectData>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Broadcast the click event with this card's data
            OnCardClicked?.Invoke(_cardObjectData);

            //Toggle card selection through the PlayerController
            if (!_cardObjectData.isSelected)
            {
                Owner.SelectCard(_cardObjectData);
            }
            else
            {
                Owner.DeselectCard(_cardObjectData);
            }
        }
    }
}
