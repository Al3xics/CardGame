using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Scriptable Objects/DeckConfig")]
public class DeckConfiguration : SerializedScriptableObject
{
    [SerializeField] private CardDeckConfig[] _cardDeckData;
    public int deckID;

    private List<CardDataSO> _cardsDeck;
    public List<CardDataSO> CardsDeck
    {
        get
        {
            if (_cardsDeck.Count <= 0)
                _cardsDeck = CreateDeck();
            return _cardsDeck;
        }
        private set => _cardsDeck = value;
    }
    
    private List<CardDataSO> CreateDeck()
    {
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
        foreach (CardDataSO cardData in _cardsDeck)
        {
            if (cardData.ID == id) return cardData;
        }
        return null;
    }
}

public class CardDeckConfig
{
    public CardDataSO CardData;
    public int quantity = 1;
}