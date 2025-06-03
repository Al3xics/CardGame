using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardsHandler : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _handTransform;
    [SerializeField] private CardDatabaseSO _cardDatabase;

    public void DrawCard()
    {
        //Temporary implementation to draw card
        GameObject cardObject = Instantiate(_cardPrefab,_handTransform);
        try
        {
            CardDataSO cardData = _cardDatabase.GetCardByID(10100);
            Texture2D cardVisual = cardData.CardVisual;
            RawImage rawImage = cardObject.GetComponentInChildren<RawImage>();
            rawImage.texture = cardVisual;
            Debug.Log($"{cardData.name} drawn");
            CardObjectData data = cardObject.GetComponent<CardObjectData>();
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

    public void PlayCard()
    {
        //Implement play card
        Debug.Log("Card played");

    }

    public void DistributeCards()
    {
        //Implement DistributeCards
        Debug.Log("Card distributed");
    }

}
