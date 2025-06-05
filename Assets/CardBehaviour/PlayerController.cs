using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [HideInInspector] public CardObjectData ActiveCard;
    [SerializeField] private EventSystem _inputEvent;
    [SerializeField] private HandManager _handManager;

    static public int _playerPA;

    public static event Action OnCardUsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

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

    public void SelectCard(CardObjectData card, bool cardHasTarget = false)
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

    public void DeselectCard(CardObjectData card)
    {
        //Implement deselect card
        Debug.Log("Card deselected");

        TweeningManager.CardDown(card.gameObject.transform);
        card.isSelected = false;
    }

    public void SelectTarget()
    {
        //Implement select target
        //Target selection
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
        ActiveCard.Effect.Apply();

        HandleUsedCard();
        CheckPA();

    }

    public void HandleUsedCard()
    {
        //Use _playerPA
        _playerPA--;

        //Remove the card from the hand
        _handManager.Discard(ActiveCard.gameObject);

        Destroy(ActiveCard.gameObject);
        //Placeholder for sending card lacking to server        
        OnCardUsed.Invoke();
    }

    public void CheckPA()
    {
        //Check player PA
        if (_playerPA > 0)
            return;

        else
        {
            Debug.Log("Turn is over");
            //Send informations to server
            //Notify that turn is over
            //Send IDs of played cards to server
            Debug.Log("Sending to server");
            _inputEvent.enabled = false;
        }
    }
}
