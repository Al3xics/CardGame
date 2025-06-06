using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Wendogo;


namespace Wendogo
{
    public class CardClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int _cardIndex;

        private CardObjectData _cardObjectData;

        public static event Action<CardObjectData> OnCardClicked;

        public PlayerController Owner { get; set; }

        private void Awake()
        {
            _cardObjectData = GetComponent<CardObjectData>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardClicked?.Invoke(_cardObjectData);

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