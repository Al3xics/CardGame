using NUnit.Framework;
using Sirenix.Reflection.Editor;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Scriptable Objects/DeckConfig")]
class DeckConfiguration : ScriptableObject
{
    public CardDeckConfig[] cardDeckData;

    public List<CardDataSO> CreateDeck()
    {
        List<CardDataSO> list = new List<CardDataSO>();
        foreach (CardDeckConfig data in cardDeckData)
        {
            for (int i = 0; i < data.quantity; i++)
            {
                list.Add(data.CardData);
            }
        }
        return list;
    }
}

[System.Serializable]
class CardDeckConfig
{
    public CardDataSO CardData;
    public int quantity = 1;
}