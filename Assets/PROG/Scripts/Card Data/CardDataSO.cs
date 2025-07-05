using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Wendogo;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardDataSO : ScriptableObject
{
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
    public bool isPassive; //Whether the card is passive or not
    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool isGroup; //Whether the card is passive or not

    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool HasTarget; //Indicates whether the card needs a target to be played

    [VerticalGroup("CardData/Stats")]
    [InfoBox("@_priorityIndexMessage", InfoMessageType.Warning, "@_showWarning")]
    [InfoBox("@_priorityIndexMessage", InfoMessageType.Error, "@_showError")]
    [MinValue(0), OnValueChanged("ValidatePriorityIndex")]
    public int nightPriorityIndex = 0;
    private bool _hasPriorityIndexConflict = false;
    private string _priorityIndexMessage = "";
    private bool _showWarning = false;
    private bool _showError = false;
    
    [VerticalGroup("CardData/Stats")]
    [InfoBox("Turns Remaining '-1' means that the card has no limit on the number of turns it can be played.")]
    [ShowIf("isPassive")]
    [MinValue(-1)]
    public int turnsRemaining = -1;

    [VerticalGroup("CardData/Left"), LabelWidth(200), MinValue(10100), MaxValue(10199)]
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

    private void ValidatePriorityIndex()
    {
#if UNITY_EDITOR
        var allCardData = AssetDatabase.FindAssets("t:CardDataSO")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CardDataSO>)
            .Where(so => so != null && so != this)
            .ToList();

        var conflicting = allCardData.Where(card => card.nightPriorityIndex == this.nightPriorityIndex).ToList();

        if (nightPriorityIndex == 0 && conflicting.Count > 0)
        {
            _hasPriorityIndexConflict = true;
            _priorityIndexMessage = $"The value 0 is shared with {conflicting.Count} other card(s). Execution order will not be guaranteed.";
            _showWarning = true;
            _showError = false;
        }
        else if (nightPriorityIndex != 0 && conflicting.Count > 0)
        {
            _hasPriorityIndexConflict = true;
            _priorityIndexMessage = $"The {nightPriorityIndex} value is already in use by: {string.Join(", ", conflicting.Select(c => c.Name))}. It has been reset to 0.";
            nightPriorityIndex = 0;
            _showError = true;
            _showWarning = false;
            EditorUtility.SetDirty(this);
        }
        else
        {
            _hasPriorityIndexConflict = false;
            _priorityIndexMessage = "";
            _showError = false;
            _showWarning = false;
        }
#endif
    }
}
