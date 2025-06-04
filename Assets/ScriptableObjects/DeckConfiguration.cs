using NUnit.Framework;
using Sirenix.OdinInspector;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Scriptable Objects/DeckConfig")]
class DeckConfiguration : SerializedScriptableObject
{
    public CardDeckConfig[] CardDeckData;

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