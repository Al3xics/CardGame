using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public enum SortField
{
    ID,
    Name
}

public enum SortOrder
{
    Ascending,
    Descending
}

[System.Flags]
public enum CardFilterType
{
    All         = Passive | Active | Group | HasTarget,
    Passive     = 1 << 0,
    Active      = 1 << 1,
    Group       = 1 << 2,
    HasTarget   = 1 << 3,
    None        = 0
}


[CreateAssetMenu(fileName = "CardDatabase", menuName = "Scriptable Objects/CardDatabase")]
[HideMonoScript]
public class CardDatabaseSO : ScriptableObject
{
    #region Generate IDs

#if UNITY_EDITOR
    [Button("Re-generate all Cards IDs", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 1f)]
    [PropertyOrder(-10), PropertySpace(10, 30)]
    private void GenerateAllIDs()
    {
        var allCards = AssetDatabase.FindAssets("t:CardDataSO")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CardDataSO>)
            .Where(c => c != null)
            .ToList();

        int updated = 0;
        foreach (var card in allCards)
        {
            int previous = card.ID;
            card.GenerateID();
            if (card.ID != previous)
                updated++;
        }

        AssetDatabase.SaveAssets();
    }
#endif

    #endregion

    #region Tools

    [BoxGroup("Tools"), PropertyOrder(-9)]
    [EnumToggleButtons, LabelText("Order"), LabelWidth(70)]
    [OnValueChanged(nameof(SortCards))]
    public SortOrder sortOrder = SortOrder.Ascending;

    [BoxGroup("Tools")]
    [EnumToggleButtons, LabelText("Sort By"), LabelWidth(70)]
    [OnValueChanged(nameof(SortCards))]
    public SortField sortField = SortField.ID;
    
    [BoxGroup("Tools")]
    [EnumToggleButtons, LabelText("Filter"), LabelWidth(70)]
    public CardFilterType filter = CardFilterType.All;

    private void SortCards()
    {
        if (cardList == null || cardList.Count == 0)
        {
            Debug.LogWarning("CardList is empty or null.");
            return;
        }

        IEnumerable<CardDataSO> sorted;

        switch (sortField)
        {
            case SortField.ID:
                sorted = cardList.OrderBy(card => card.ID);
                break;
            case SortField.Name:
                sorted = cardList.OrderBy(card => card.Name);
                break;
            default:
                sorted = cardList;
                break;
        }

        // Inverser si ordre décroissant
        if (sortOrder == SortOrder.Descending)
            sorted = sorted.Reverse();

        cardList = sorted.ToList();
    
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
    
    #endregion
    
    #region Variables

    [FormerlySerializedAs("CardList")]
    [PropertySpace(30, 30)]
    [PropertyOrder(0)]
    [TableList, Searchable]
    [FilteredCardList("filter")]
    public List<CardDataSO> cardList = new();

    //Internal dictionary for quick ID lookup
    private Dictionary<int, CardDataSO> _cardDictionary = new();

    #endregion

    #region Basic Methods

    private void OnEnable()
    {
        //Called when ScriptableObject is loaded
        Initialize();
    }

    private void Initialize()
    {
        //Populate the dictionary from the list
        foreach (CardDataSO card in cardList)
        {
            //Add cards to the dictionary for quick ID-based access
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

    public CardDataSO GetCardByName(string cardName)
    {
        //Find a card by its display name
        foreach (CardDataSO card in cardList)
        {
            if (card.Name == name)
            {
                return card;
            }
        }

        //Return null if no match is found
        return null;
    }

    #endregion
}
