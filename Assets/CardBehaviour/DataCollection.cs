using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Wendogo
{

    public class DataCollection : MonoBehaviour
    {
        public DeckConfiguration ResourcesDeck;
        public DeckConfiguration ActionDeck;
        List<CardDataSO> cardList = new List<CardDataSO>();



        [SerializeField] private Button deckButton;

        private void Start()
        {
            deckButton.onClick.AddListener(() => Debug.Log($"{cardList}"));
        }

        public Dictionary<int, CardDataSO> GetDeck(int deckID)
        {
            if (ResourcesDeck.DeckID == deckID)
                return ResourcesDeck.DeckKeyValues;
            else if(ActionDeck.DeckID== deckID) return ActionDeck.DeckKeyValues;
            return null;
        }

        public CardDataSO GetCardByID(int id)
        {
            if (ResourcesDeck.DeckKeyValues.TryGetValue(id, out CardDataSO card))
            {
                return card;
            }
            return null;
        }
    }

}