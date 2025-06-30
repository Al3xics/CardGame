using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Wendogo
{

    public class DataCollection : MonoBehaviour
    {
        public CardDatabaseSO cardDatabase;
        public DeckConfiguration resourcesDeck;
        public DeckConfiguration actionDeck;

        // Runtime versions of the decks
        public DeckConfiguration RuntimeResourcesDeck { get; private set; }
        public DeckConfiguration RuntimeActionDeck { get; private set; }
        
        private void Awake()
        {
            // Clone the ScriptableObjects at runtime to avoid modifying the original
            RuntimeResourcesDeck = Instantiate(resourcesDeck);
            RuntimeActionDeck = Instantiate(actionDeck);
        }
        
        public List<CardDataSO> GetDeck(int deckID)
        {
            if (RuntimeResourcesDeck.deckID == deckID)
                return RuntimeResourcesDeck.CardsDeck;
            else if(RuntimeActionDeck.deckID== deckID)
                return RuntimeActionDeck.CardsDeck;
            
            return null;
        }
    }
}