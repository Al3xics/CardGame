using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Wendogo
{

    public class DataCollection : MonoBehaviour
    {
        #region Session Manager Instance
        
        private static DataCollection _instance;
        public static DataCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<DataCollection>();
                    if (_instance == null) Debug.LogError("DataCollection instance is missing in the scene and has not been set.");
                }
                return _instance;
            }
            private set => _instance = value;
        }
        
        #endregion

        #region Variables

        public CardDatabaseSO cardDatabase;
        public DeckConfiguration resourcesDeck;
        public DeckConfiguration actionDeck;

        #endregion

        // Runtime versions of the decks
        private List<CardDataSO> _resourceDeck = new();
        public List<CardDataSO> RuntimeResourcesDeck
        {
            get
            {
                //Lazy initialization of the deck
                if (_resourceDeck.Count <= 0)
                    _resourceDeck = CreateDeck(resourcesDeck.cardDeckData);
        
                return _resourceDeck;
            }
            set => _resourceDeck = value;
        }
        
        private List<CardDataSO> _actionDeck = new();
        public List<CardDataSO> RuntimeActionDeck
        {
            get
            {
                //Lazy initialization of the deck
                if (_actionDeck.Count <= 0)
                    _actionDeck = CreateDeck(actionDeck.cardDeckData);
        
                return _actionDeck;
            }
            set => _actionDeck = value;
        }
        
        private List<CardDataSO> CreateDeck(CardDeckConfig[] cardDeckData)
        {
            //Construct a list of card instances based on their quantity settings
            var list = new List<CardDataSO>();
            foreach (CardDeckConfig data in cardDeckData)
            {
                for (int i = 0; i < data.quantity; i++)
                {
                    list.Add(data.CardData);
                }
            }
        
            return list;
        }
        
        public List<CardDataSO> GetDeck(int deckID)
        {
            if (resourcesDeck.deckID == deckID)
                return RuntimeResourcesDeck;
            else if(actionDeck.deckID== deckID)
                return RuntimeActionDeck;
            
            return null;
        }
    }
}