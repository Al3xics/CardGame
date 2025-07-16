using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Wendogo;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
[HideMonoScript]
public class CardDataSO : ScriptableObject
{
    #region Variables

    [Title("Card Visual")]
    [HorizontalGroup("CardData", 100), VerticalGroup("CardData/Left", PaddingBottom = 5)]
    [PreviewField(100, ObjectFieldAlignment.Left), HideLabel]
    public Texture2D CardVisual; //Preview the visual in the editor

    [Title("Card Stats")]
    [VerticalGroup("CardData/Stats", 10), LabelWidth(75)]
    public string Name; //Name of the card

    [VerticalGroup("CardData/Stats"), LabelWidth(75)]
    public CardEffect CardEffect; //Reference to the card's effect

    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    [OnValueChanged("GenerateID")]
    public bool isPassive; //Whether the card is passive or not
    private bool _previousIsPassive;
    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool isGroup; //Whether the card is passive or not

    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool HasTarget; //Indicates whether the card needs a target to be played

    [VerticalGroup("CardData/Stats")]
    [InfoBox("@_priorityIndexMessage", InfoMessageType.Info, "@_showWarning")]
    [InfoBox("@_priorityIndexMessage", InfoMessageType.Error, "@_showError")]
    [MinValue(0), ShowIf("isNotPassive"), OnValueChanged("ValidatePriorityIndex")]
    public int nightPriorityIndex = 0;
    private string _priorityIndexMessage = "";
    private bool _showWarning = false;
    private bool _showError = false;
    private bool isNotPassive => !isPassive;
    
    [VerticalGroup("CardData/Stats")]
    [InfoBox("Turns Remaining '-1' means that the card has no limit on the number of turns it can be played.")]
    [MinValue(-1), ShowIf("isPassive")]
    public int turnsRemaining = -1;

    [VerticalGroup("CardData/Left"), LabelWidth(200), ReadOnly]
    [ShowIf("Toggle"), HideLabel]
    public int ID; //Unique identifier for this card

    private string _buttonName = "ID";
    [HideInInspector]
    public bool Toggle; //Controls visibility of the ID field

    [VerticalGroup("CardData/Left")]
    [Button("$_buttonName", ButtonSizes.Small, Stretch = false, ButtonAlignment = 0.5f)]
    private void IDButton()
    {
        // Toggles the visibility of the ID field for this card
        this.Toggle = !this.Toggle;
    }

    #endregion

    #region Basic Methods

#if UNITY_EDITOR
    private void Awake()
    {
        _previousIsPassive = isPassive;
        if (ShouldSkipProcessing()) GenerateID();
    }

    // private void OnValidate()
    // {
    //     if (ShouldSkipProcessing()) return;
    //     
    //     // Check if the "isPassive" field has been changed
    //     if (_previousIsPassive != isPassive)
    //     {
    //         // Update _previousIsPassive to reflect the new state
    //         _previousIsPassive = isPassive;
    //         GenerateID();
    //     }
    // }

    private bool ShouldSkipProcessing()
    {
        return EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private void ValidatePriorityIndex()
    {
        var allCardData = AssetDatabase.FindAssets("t:CardDataSO")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CardDataSO>)
            .Where(so => so != null && so != this)
            .ToList();

        var conflicting = allCardData.Where(card => card.nightPriorityIndex == this.nightPriorityIndex).ToList();

        if (nightPriorityIndex == 0 && conflicting.Count > 0)
        {
            _priorityIndexMessage = $"The value 0 is shared with {conflicting.Count} other card(s). Execution order will not be guaranteed.";
            _showWarning = true;
            _showError = false;
        }
        else if (nightPriorityIndex != 0 && conflicting.Count > 0)
        {
            _priorityIndexMessage = $"The {nightPriorityIndex} value is already in use by: {string.Join(", ", conflicting.Select(c => c.Name))}. It has been reset to 0.";
            nightPriorityIndex = 0;
            _showError = true;
            _showWarning = false;
            EditorUtility.SetDirty(this);
        }
        else
        {
            _priorityIndexMessage = "";
            _showError = false;
            _showWarning = false;
        }
    }
    
    public void GenerateID()
    {
        // Skip if Unity is importing assets
        if (ShouldSkipProcessing()) return;
        
        int prefix = isPassive ? 2 : 1;
        int min = prefix * 1000 + 1;
        int max = prefix * 1000 + 999;

        var allCards = AssetDatabase.FindAssets("t:CardDataSO")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CardDataSO>)
            .Where(c => c != null && c != this && c.isPassive == this.isPassive)
            .ToList();

        var usedIDs = allCards.Select(c => c.ID).ToHashSet();

        for (int candidate = min; candidate <= max; candidate++)
        {
            if (!usedIDs.Contains(candidate))
            {
                ID = candidate;
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                return;
            }
        }

        Debug.LogError($"[CardDataSO] Aucun ID disponible pour les cartes {(isPassive ? "passives" : "actives")}");
    }

#endif
    
    public static CardDataSO Clone(CardDataSO cardData)
    {
        // Create a clone of `CardDataSO` in memory
        CardDataSO copy = Instantiate(cardData);
        var turn = cardData.turnsRemaining;
        copy.turnsRemaining = turn == -1 ? turn : turn * 2;
        
        return copy;
    }

    #endregion
}
