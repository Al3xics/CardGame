using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Scriptable Objects/CardDatabase")]
public class CardDatabaseSO : ScriptableObject
{
    public List<CardDataSO> CardList = new List<CardDataSO>();

    private Dictionary<int, CardDataSO> _cardDictionary = new Dictionary<int, CardDataSO>();

    public void Initialize()
    {
        foreach (CardDataSO card in CardList)
        {
            _cardDictionary.Add(card.ID, card);
        }
    }

    public CardDataSO GetCardByID(int id)
    {
        if (_cardDictionary.TryGetValue(id, out CardDataSO card))
        {
            return card;
        }
        return null;
    }

    public List<CardDataSO> GetCardsByType(CardTypeSO type)
    {
        List<CardDataSO> cardsOfType = new List<CardDataSO>();

        foreach (CardDataSO card in CardList)
        {
            if (card.CardType == type)
            {
                cardsOfType.Add(card);
            }
        }
        return cardsOfType;
    }

    public CardDataSO GetCardByName(string name)
    {
        foreach (CardDataSO card in CardList)
        {
            if (card.Name == name)
            {
                return card;
            }
        }
        return null;
    }
}
