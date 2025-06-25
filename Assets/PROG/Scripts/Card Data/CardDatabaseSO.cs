using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Wendogo;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Scriptable Objects/CardDatabase")]
public class CardDatabaseSO : ScriptableObject
{
    [TableList, Searchable]
    public List<CardDataSO> CardList = new List<CardDataSO>();

    //Internal dictionary for quick ID lookup
    private Dictionary<int, CardDataSO> _cardDictionary = new Dictionary<int, CardDataSO>();

    private void OnEnable()
    {
        //Called when ScriptableObject is loaded
        Initialize();
    }

    public void Initialize()
    {
        //Populate the dictionary from the list
        foreach (CardDataSO card in CardList)
        {
            //Add cards to dictionary for quick ID-based access
            _cardDictionary.Add(card.ID, card);
        }
    }

    public CardDataSO GetCardByID(int id)
    {
        //Retrieve a card by its unique ID
        if (_cardDictionary.TryGetValue(id, out CardDataSO card))
        {
            return card;
        }

        //Return null if not found
        return null;
    }

    public CardDataSO GetCardByName(string name)
    {
        //Find a card by its display name
        foreach (CardDataSO card in CardList)
        {
            if (card.Name == name)
            {
                return card;
            }
        }

        //Return null if no match is found
        return null;
    }
}
