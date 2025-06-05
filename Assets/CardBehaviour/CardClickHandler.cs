using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int _cardIndex;

    private CardObjectData _cardObjectData;

    public static event Action<CardObjectData> OnCardClicked;

    private void Awake()
    {
        _cardObjectData = GetComponent<CardObjectData>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(_cardObjectData);

        if (!_cardObjectData.isSelected)
        {
            PlayerController.Instance.SelectCard(_cardObjectData);
        }
        else 
        {
            PlayerController.Instance.DeselectCard(_cardObjectData);
        }
    }
}


