using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Scriptable Objects/DeckConfig")]
class DeckConfiguration : SerializedScriptableObject
{
    public CardDeckConfig[] CardDeckData;
    public int DeckID;

    [HideInInspector] public Dictionary<int, CardDataSO> DeckKeyValues;


    public void CreateDeckDictionnary()
    {
        foreach (CardDataSO card in CreateDeck())
        {
            DeckKeyValues.Add(card.ID, card);
        }
    }

    public CardDataSO GetCardByID(int id)
    {
        if (DeckKeyValues.TryGetValue(id, out CardDataSO card))
        {
            return card;
        }
        return null;
    }
    public List<CardDataSO> CreateDeck()
    {
        List<CardDataSO> list = new List<CardDataSO>();
        foreach (CardDeckConfig data in CardDeckData)
        {
            for (int i = 0; i < data.quantity; i++)
            {
                list.Add(data.CardData);
            }
        }
        return list;
    }
}

class CardDeckConfig
{
    public CardDataSO CardData;
    public int quantity = 1;
}