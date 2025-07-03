using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Wendogo;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Scriptable Objects/DeckConfig")]
public class DeckConfiguration : SerializedScriptableObject
{
    [SerializeField] private CardDeckConfig[] _cardDeckData; //Array of cards and their quantities for this deck
    public int deckID; //Identifier for this specific deck configuration

    private List<CardDataSO> _cardsDeck; //List of instantiated card references

    public List<CardDataSO> CardsDeck
    {
        get
        {
            //Lazy initialization of the deck
            if (_cardsDeck.Count <= 0)
                _cardsDeck = CreateDeck();

            return _cardsDeck;
        }
        set => _cardsDeck = value;
    }

    private List<CardDataSO> CreateDeck()
    {
        //Construct a list of card instances based on their quantity settings
        var list = new List<CardDataSO>();
        foreach (CardDeckConfig data in _cardDeckData)
        {
            for (int i = 0; i < data.quantity; i++)
            {
                list.Add(data.CardData);
            }
        }

        return list;
    }

    public CardDataSO GetCardDataByID(int id)
    {
        //Lookup for a card in the deck by its ID
        foreach (CardDataSO cardData in _cardsDeck)
        {
            if (cardData.ID == id)
                return cardData;
        }

        //Return null if no match is found
        return null;
    }
}

//Configuration structure for each card entry in the deck
[System.Serializable]
public class CardDeckConfig
{
    public CardDataSO CardData; //Reference to the card asset
    public int quantity = 1;    //Number of times this card appears in the deck
    //Can add more variable if necessary: Enum deck type?
}
