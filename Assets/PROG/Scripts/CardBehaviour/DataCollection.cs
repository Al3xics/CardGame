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

        public List<CardDataSO> GetDeck(int deckID)
        {
            if (resourcesDeck.deckID == deckID)
                return resourcesDeck.CardsDeck;
            else if(actionDeck.deckID== deckID)
                return actionDeck.CardsDeck;
            
            return null;
        }
    }
}