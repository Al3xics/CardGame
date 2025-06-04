using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int _cardIndex;

    private CardObjectData _cardObjectData;

    private void Awake()
    {
        _cardObjectData = GetComponent<CardObjectData>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_cardObjectData.isSelected)
        {
            PlayerController.SelectCard(_cardObjectData);
        }
        else 
        {
            PlayerController.DeselectCard(_cardObjectData);
        }
    }
}


