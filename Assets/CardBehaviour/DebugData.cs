using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class DebugData : MonoBehaviour
{
    [SerializeField] private DeckConfiguration deckConfiguration;
    List<CardDataSO> cardList = new List<CardDataSO>();

    [SerializeField] private Button deckButton;

    private void Start()
    {
        deckButton.onClick.AddListener(() => Debug.Log($"{GetDeckID()}"));
    }

    public int GetDeckID()
    {
        return deckConfiguration.DeckID;
    }

    public CardDataSO GetCardByID(int id)
    {
        if (deckConfiguration.DeckKeyValues.TryGetValue(id, out CardDataSO card))
        {
            return card;
        }
        return null;
    }
}
