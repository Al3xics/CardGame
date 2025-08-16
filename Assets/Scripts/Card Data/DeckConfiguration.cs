using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Scriptable Objects/DeckConfig")]
[HideMonoScript]
public class DeckConfiguration : ScriptableObject
{
    #region Variables

    public int deckID; //Identifier for this specific deck configuration
    [Space(10)]
    public CardDeckConfig[] cardDeckData; //Array of cards and their quantities for this deck

    #endregion
}

//Configuration structure for each card entry in the deck
[System.Serializable]
public class CardDeckConfig
{
    public CardDataSO CardData; //Reference to the card asset
    public int quantity = 1;    //Number of times this card appears in the deck
    //Can add more variable if necessary: Enum deck type?
}
