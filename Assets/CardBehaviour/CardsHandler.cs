using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardsHandler : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _handTransform;
    [SerializeField] private CardDatabaseSO _cardDatabase;

    //Placeholder methods for receiving cards from the server 
    public void ApplyCardData(GameObject card)
    {
        try
        {
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
