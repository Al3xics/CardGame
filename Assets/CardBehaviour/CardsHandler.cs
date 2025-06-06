using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class CardsHandler : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _handTransform;
    [SerializeField] private CardDatabaseSO _cardDatabase;
    private PlayerController _owner;

    private void Start()
    {
        _owner = GetComponent<PlayerController>();
    }

    //Placeholder methods for receiving cards from the server
    //Add ID reveived by server in get card by ID
    public void ApplyCardData(GameObject card)
    {
        try
        {
            CardClickHandler clickHandler = card.GetComponent<CardClickHandler>();
            clickHandler.Owner = _owner;
            CardDataSO cardData = _cardDatabase.GetCardByID(10100);
            Texture2D cardVisual = cardData.CardVisual;
            RawImage rawImage = card.GetComponentInChildren<RawImage>();
            rawImage.texture = cardVisual;
            Debug.Log($"{cardData.name} drawn");
            CardObjectData data = card.GetComponent<CardObjectData>();
            data.Card = cardData;
        }
        catch (System.ArgumentNullException e)
        {
            Debug.Log(e.Message);
            throw;
        }
    }

    public void MakeDeck(CardDatabaseSO database)
    {
        //Use database to get cards and construct deck
        //Just use the database as the deck?
    }

}
