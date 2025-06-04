using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public static CardObjectData ActiveCard;
    [SerializeField] private EventSystem _inputEvent;
    [SerializeField] private HandManager _handManager;

    static public int _playerPA;

    public static event Action OnCardUsed;

    private void Awake()
    {
        _playerPA = 2;
    }

    public void EnableInput()
    {
        _inputEvent.enabled = true;

        //Implement enable input
        Debug.Log("Input enabled");
    }

    public static void SelectCard(CardObjectData card, bool cardHasTarget = false)
    {
        //Implement select card
        if (cardHasTarget)
            SelectTarget();

        if (ActiveCard != null)
            DeselectCard(ActiveCard);

        ActiveCard = card;

        TweeningManager.CardUp(card.gameObject.transform);
        card.isSelected = true;

        Debug.Log("Card selected");
    }

    public static void DeselectCard(CardObjectData card)
    {
        //Implement select card

        Debug.Log("Card deselected");

        TweeningManager.CardDown(card.gameObject.transform);
        card.isSelected = false;
    }

    public static void SelectTarget()
    {
        //Implement select target
        Debug.Log("Target selected");
        //ConfirmPlay();
    }

    public static void BurnCard()
    {
        //Implement burn card
    }

    public async void ConfirmPlay()
    {
        //Implement confirm play
        if (ActiveCard == null)
            return;


        Debug.Log("Confirm play");

        //Apply effect
        //Not needed?
        ActiveCard.Effect.Apply();

        //Use _playerPA
        _playerPA--;

        //Placeholder for sending card lacking to server        

        _handManager.Discard(ActiveCard.gameObject);
        
        Destroy(ActiveCard.gameObject);
        OnCardUsed.Invoke();


        //Check player PA
        if (_playerPA > 0)
            return;

        //Send informations to server
        //Notify that turn is over
        //Send IDs of played cards to server
        Debug.Log("Sending to server");


    }
}
